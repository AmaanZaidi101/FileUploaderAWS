using FileUploader.Application.Enums;

namespace FileUploader.Infrastructure.AWS
{
	public sealed class S3UploadProgress
	{
		public enUploadState State { get; set; } = enUploadState.None;
        public int Percentage { get; init; }
		public long TransferredBytes { get; init; }
		public long TotalBytes { get; init; }
	}

}
