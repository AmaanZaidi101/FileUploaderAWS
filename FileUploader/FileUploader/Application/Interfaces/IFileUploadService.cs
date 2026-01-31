namespace FileUploader.Application.Interfaces
{
    public interface IFileUploadService
    {
        public void SaveChunks(string lessonId, string fileId, int chunkIndex, IFormFile chunk);

        public string? MergeChunks(string lessonId, string fileId, string fileType);
    }
}
