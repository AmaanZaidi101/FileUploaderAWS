namespace FileUploader.Application.DTOs
{
    public class FileUploadCompleteDTO
    {
        public Guid FileId { get; init; }
        public Guid LessonId { get; init; }
		public string FileType { get; init; }
	}
}
