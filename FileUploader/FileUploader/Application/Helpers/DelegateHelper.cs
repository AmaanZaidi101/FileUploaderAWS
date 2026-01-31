using FileUploader.Api.Hubs;
using FileUploader.Application.Enums;
using FileUploader.Infrastructure.AWS;
using Microsoft.AspNetCore.SignalR;

namespace FileUploader.Application.Helpers
{
    public class DelegateHelper
    {
        public static Func<S3UploadProgress, Task> CreateS3ProgressDelegate(IHubContext<UploadHub> hub, Guid fileId)
        {
			return async (S3UploadProgress progress) =>
			{

				UploadHub.TryGetConnection(fileId, out var connectionId);

				if (string.IsNullOrWhiteSpace(connectionId))
					return;

				switch (progress.State)
				{
					case enUploadState.None:
						break;
					case enUploadState.Initializing:
						await hub.Clients.Client(connectionId).SendAsync("UploadProgress", "Initialising");
						break;
					case enUploadState.Uploading:
						await hub.Clients.Client(connectionId).SendAsync("UploadProgress", $"{progress.Percentage} %");
						break;
					case enUploadState.Completed:
						await hub.Clients.Client(connectionId).SendAsync("UploadComplete", true);
						break;
					case enUploadState.Failed:
						await hub.Clients.Client(connectionId).SendAsync("UploadComplete", false);
						break;
					default:
						break;
				}
			};
		}
    }
}
