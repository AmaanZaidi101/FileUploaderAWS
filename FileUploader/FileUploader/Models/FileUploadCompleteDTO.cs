namespace FileUploader.Models
{
    public class FileUploadCompleteDTO
    {
        public Guid LessonId { get; init; }
        public string FileType { get; init; }
	}
}
