namespace FileUploader.Infrastructure.AWS
{
    public interface IS3Uploader
    {
        Task Upload(string filePath, Func<S3UploadProgress, Task> onProgress, CancellationToken ct);
    }
}
