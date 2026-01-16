
import { use, useRef, useState } from "react"
import { Button, Form, Modal } from "react-bootstrap"
import { FormLabel } from "react-bootstrap"
export const Uploader = () => {
    const CHUNK_SIZE: number = 500 * 1024;
    const API_URL = 'https://localhost:7070/api';
    const lessonId = 'db6bfdfd-5778-40e3-8464-6b149e58f0f2';
    const [validated, setValidated] = useState(false);
    const [error, setError] = useState('');
    const [fileName, setFileName] = useState('');
    const [file, setFile] = useState<File | null>(null);
    const [modalShow, setModalShow] = useState(false);
    const [modalMessage, setModalMessage] = useState('');
    const [modalColor, setModalColor] = useState('blue');
    const [modalBtnDisabled, setModalBtnDisabled] = useState(true);

    const fileInputRef = useRef<HTMLInputElement>(null);

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

        }
        else {
            setModalColor('green');
        }
        setModalBtnDisabled(false);
        fileInputRef.current!.value = ''
        setFile(null);
        setFileName('');
    }

    const uploadChunks = async (chunks: Blob[]) => {
        for (let i = 0; i < chunks.length; i++) {

            const formData = buildFormData(
                chunks[i],
                { lessonId, chunkIndex: i, totalChunks: chunks.length });
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
        return true;
    }


    function buildFormData(chunk: any, meta: any) {

        const formData = new FormData();
        formData.append("lessonId", meta.lessonId);
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

    }
    const onModalHide = () => {
        setModalMessage('');
        setModalBtnDisabled(true);
        setModalShow(false);
    }
    return (
        <>
            <Form noValidate validated={validated} onSubmit={handleSubmit}>
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


