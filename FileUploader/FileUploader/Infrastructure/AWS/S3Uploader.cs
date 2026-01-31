
using Amazon.S3.Transfer;
using FileUploader.Application.Enums;
using FileUploader.Infrastructure.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace FileUploader.Infrastructure.AWS
{
    public class S3Uploader : IS3Uploader
    {
        private readonly IS3ClientFactory _clientFactory;
        private readonly AwsOptions _options;

        public S3Uploader(IS3ClientFactory clientFactory, IOptions<AwsOptions> options)
        {
            _clientFactory = clientFactory;
            _options = options.Value;
        }

        public async Task Upload(string filePath, Func<S3UploadProgress, Task> onProgress, CancellationToken ct)
        {
			//for (int i = 0; i <= 100; i += 10)
			//{
			//	await Task.Delay(1000);
			//	var progress = new S3UploadProgress
			//	{
			//		Percentage = i,
			//		TransferredBytes = i,
			//		TotalBytes = 100,
			//		State = enUploadState.Uploading
			//	};
   //             await onProgress(progress);
			//}
			//var progress1 = new S3UploadProgress
			//{
			//	Percentage = 100,
			//	TransferredBytes = 100,
			//	TotalBytes = 100,
			//	State = enUploadState.Completed
			//};
   //         await onProgress(progress1);
   //         return;
			using (Stream stream = new FileStream(filePath, FileMode.Open))
            {
                using (var s3 = _clientFactory.Create())
                {
                    var fileName = Path.GetFileName(filePath);

					var key = S3ObjectKeyBuilder.Build(fileName);

                    var transferUtility = new TransferUtility(s3);

                    var request = new TransferUtilityUploadRequest
                    {
                        BucketName = _options.BucketName,
                        Key = key,
                        InputStream = stream,
                        AutoCloseStream = false
                    };

                    request.UploadInitiatedEvent += async (_, e) =>
                    {
                        var progress = new S3UploadProgress
                        {
                            Percentage = 0,
                            TransferredBytes = 0,
                            TotalBytes = e.TotalBytes,
                            State = enUploadState.Initializing
                        };
                    };

                    request.UploadProgressEvent += async (_, e) =>
                    {
                        var progress = new S3UploadProgress
                        {
                            Percentage = (int)(e.TransferredBytes / e.TotalBytes * 100),
                            TransferredBytes = e.TransferredBytes,
                            TotalBytes = e.TotalBytes,
                            State = enUploadState.Uploading
                        };

                        await onProgress(progress);
                    };

					request.UploadCompletedEvent += async (_, e) =>
					{
						var progress = new S3UploadProgress
						{
							Percentage = 100,
							TransferredBytes = e.TransferredBytes,
							TotalBytes = e.TotalBytes,
							State = enUploadState.Completed
						};

						await onProgress(progress);
					};

					request.UploadFailedEvent += async (_, e) =>
					{
						var progress = new S3UploadProgress
						{
							Percentage = (int)(e.TransferredBytes / e.TotalBytes * 100),
							TransferredBytes = e.TransferredBytes,
							TotalBytes = e.TotalBytes,
							State = enUploadState.Failed
						};

						await onProgress(progress);
					};

					await transferUtility.UploadAsync(request, ct);
                }
            }
        }
    }
}
