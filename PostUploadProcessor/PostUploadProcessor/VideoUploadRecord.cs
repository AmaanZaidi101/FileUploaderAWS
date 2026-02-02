using Amazon.DynamoDBv2.DataModel;

namespace PostUploadProcessor
{
	public class VideoUploadRecord
	{
		[DynamoDBHashKey]
		public string FileId { get; set; } = null!;
		public string Bucket { get; set; } = null!;
		public string S3Key { get; set; } = null!;
		public string LessonId { get; set; } = null!;
		public string ContentType { get; set; } = null!;
		public long SizeBytes { get; set; }
		public string Status { get; set; } = "Uploaded";
		public DateTime UploadedAt { get; set; }
	}

}
