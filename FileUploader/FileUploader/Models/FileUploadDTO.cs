namespace FileUploader.Models
{
    public class FileUploadDTO
    {
        public Guid FileId { get; set; }
        public Guid LessonId { get; set; }
		public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public IFormFile Chunk { get; set; } = default!;
    }
}
