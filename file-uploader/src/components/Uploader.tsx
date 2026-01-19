
import { useRef, useState, useEffect } from "react"
import { Button, Form, Modal } from "react-bootstrap"
import { FormLabel } from "react-bootstrap"
import { HubConnection } from "@microsoft/signalr"
import * as signalr from '@microsoft/signalr'


export const Uploader = () => {
    const CHUNK_SIZE: number = 500 * 1024;
    const API_URL = 'https://localhost:7070/api';
    const lessonId = 'db6bfdfd-5778-40e3-8464-6b149e58f0f2';
    const [locUploadComplete, setlocUploadComplete] = useState(false);
    const [error, setError] = useState('');
    const [fileId, setFileId] = useState('');
    const [fileName, setFileName] = useState('');
    const [file, setFile] = useState<File | null>(null);
    const [modalShow, setModalShow] = useState(false);
    const [modalMessage, setModalMessage] = useState('');
    const [modalColor, setModalColor] = useState('blue');
    const [modalBtnDisabled, setModalBtnDisabled] = useState(true);

    const fileInputRef = useRef<HTMLInputElement>(null);
    const connectionRef = useRef<HubConnection>(null);

    const commonVideoTypes = [
        'video/mp4',           // Most common
        'video/webm',          // Web optimized
        'video/ogg',           // Open source
        'video/quicktime',     // Apple MOV
        'video/x-msvideo',     // AVI
    ];

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setModalColor('blue');
        setModalShow(true);
        setModalMessage('Processing');
        setModalMessage('Chunking up the video nice and good');
        const chunks = chunkFile();
        if (chunks == null)
            return;
        if (!(await uploadChunks(chunks)) || !(await uploadComplete(lessonId))) {
            setModalColor('red');
            setModalMessage("Upload Failed!!!");
            cleanUp();
        }
    }

    useEffect(() => {
        if (!locUploadComplete || !fileId)
            return;

        const connection = new signalr.HubConnectionBuilder()
            .withUrl("https://localhost:7070/uploadHub", {
                withCredentials: true
            }).withAutomaticReconnect().build();

        connection.on("UploadProgress", (percent) => {
            setModalColor("black");
            setModalMessage(`Upload to cloud: ${percent}`);
        })

        connection.on("UploadComplete", () => {
            setModalColor('green');
            setModalMessage("Uploaded to cloud");
            setModalBtnDisabled(false);
        })

        connection.start()
            .then(() => connection.invoke("RegisterUpload", fileId)
                .then(() => setModalMessage('Now uploading to cloud'))
                .then(() => trackS3Upload())
                .then(() => { connection.stop(); cleanUp(); })
                .catch(err => {
                    console.log(err);
                    connection.stop();
                    setModalColor('red');
                    setModalMessage("Upload Failed!!!");
                    cleanUp();
                }));



        connectionRef.current = connection;

        return () => {
            connection.stop();
            connectionRef.current = null;
        }
    }, [locUploadComplete])

    const cleanUp = () => {
        setFile(null);
        setFileName('');
        setFileId('');
        fileInputRef.current!.value = ''
        setModalBtnDisabled(false);

    }

    const trackS3Upload = async () => {
        try {
            const response = await fetch(`${API_URL}/fileupload/progress`, {
                method: 'POST',
                body: JSON.stringify(fileId),
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) {
                return false;
            }
        } catch (err) {
            return false;
        }

        return true;
    }

    const uploadChunks = async (chunks: Blob[]) => {
        for (let i = 0; i < chunks.length; i++) {

            const formData = buildFormData(
                chunks[i],
                { lessonId, fileId, chunkIndex: i, totalChunks: chunks.length });
            const res = await uploadChunk(formData);
            if (!res)
                return res;

            const percent = Math.round(((i + 1) / chunks.length) * 100);
            setModalMessage(`Sending to server : ${percent} % completed`);
        }
        return true;
    }

    const uploadChunk = async (formData: FormData) => {

        try {
            const response = await fetch(`${API_URL}/fileupload/chunk`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                return false;
            }
        } catch (err) {
            return false;
        }

        return true;

    }

    const uploadComplete = async (lessonId: string) => {
        const formData = new FormData();
        formData.append("lessonId", lessonId);
        formData.append("fileId", fileId);
        console.log(fileId);

        formData.append("fileType", file!.type);

        try {
            const response = await fetch(`${API_URL}/fileupload/complete`,
                {
                    method: "POST",
                    body: formData
                });

            if (!response.ok) {
                return false;
            }
        } catch (err) {
            return false;
        }

        setModalMessage(`File sent to server successfully`);
        setlocUploadComplete(true);
        return true;
    }


    function buildFormData(chunk: any, meta: any) {

        const formData = new FormData();
        formData.append("lessonId", meta.lessonId);
        formData.append("fileId", meta.fileId);
        formData.append("chunkIndex", meta.chunkIndex);
        formData.append("totalChunks", meta.totalChunks);
        formData.append("chunk", chunk, fileName);

        return formData;
    }
    const chunkFile = () => {
        if (file == null)
            return;
        const chunks: Blob[] = [];
        let offset = 0;
        while (offset < file.size) {
            let chunkSize = offset + CHUNK_SIZE > file?.size ? file?.size - offset : CHUNK_SIZE;
            let chunk = file.slice(offset, offset + chunkSize);
            chunks.push(chunk);
            offset += chunkSize;
        }
        return chunks;
    }

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setError('');
        setFileName('');
        setFile(null);
        setFileId('')
        const selectedFile = e?.target?.files?.[0] ?? null;
        if (selectedFile == null) {
            setError('Could not select file, please try again!');
            return;
        }
        if (!commonVideoTypes.includes(selectedFile?.type)) {
            setError('Invalid file type!!');
            return;
        }
        setFile(selectedFile);
        setFileName(selectedFile.name);
        setFileId(crypto.randomUUID());

    }
    const onModalHide = () => {
        setModalMessage('');
        setModalBtnDisabled(true);
        setModalShow(false);
    }
    return (
        <>
            <Form noValidate onSubmit={handleSubmit}>
                <Form.Group className="mb-3" controlId="uploader">
                    <FormLabel>Upload the lesson video</FormLabel>
                    <Form.Control
                        type="file" name="fileuploader" placeholder="No File Chosen Yet"
                        onChange={handleFileChange} ref={fileInputRef}
                    />
                </Form.Group>
                {error &&
                    (<div>
                        <span style={{ color: 'red' }}>{error}</span>
                    </div>)}
                {fileName &&
                    (<div>
                        <span style={{ color: 'blue' }}>You have selected: <strong>{fileName}</strong></span>
                    </div>)}
                <Button type="submit" className="my-2 py-2 px-5" disabled={file == null || error != ''}>
                    Upload!
                </Button>
            </Form>
            <Modal show={modalShow} onHide={onModalHide} backdrop='static' keyboard={false} >
                <Modal.Header>
                    <Modal.Title>File upload in progress</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div>
                        <strong style={{ color: modalColor }}>{modalMessage}</strong>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button type="button" className="btn-primary" style={{ background: modalColor }} onClick={onModalHide} disabled={modalBtnDisabled}>Close</Button>
                </Modal.Footer>
            </Modal>
        </>
    )
}
export default Uploader


