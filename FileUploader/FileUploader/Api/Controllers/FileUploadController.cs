using FileUploader.Api.Hubs;
using FileUploader.Application.DTOs;
using FileUploader.Application.Helpers;
using FileUploader.Application.Interfaces;
using FileUploader.Infrastructure.AWS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FileUploader.Api.Controllers
{
	[ApiController]
	[Route("/api/[controller]")]
	public class FileUploadController : ControllerBase
	{
		private readonly string _tempFolder;
		private readonly int _chunkSize;
		private readonly int _maxSize;
        private readonly IHubContext<UploadHub> _hub;

		private readonly IFileUploadService _fileUploadService;
		private readonly IS3Uploader _s3Uploader;
        public FileUploadController(IHubContext<UploadHub> hub, IFileUploadService fileUploadService, IS3Uploader s3Uploader)
        {
            _tempFolder = @"C:\\Users\\Batman\\Downloads\\Tmp\\";
            _chunkSize = 500 * 1024;
            _maxSize = 10000 * 1024;
            _hub = hub;
            _fileUploadService = fileUploadService;
            _s3Uploader = s3Uploader;
        }
        [HttpPost("chunk")]
		public async Task<IActionResult> UploadChunk([FromForm] FileUploadDTO file)
		{
			try
			{
				_fileUploadService.SaveChunks(file.LessonId.ToString(), file.FileId.ToString(), file.ChunkIndex, file.Chunk);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
			
			return Ok();
		}

		[HttpPost("complete")]
		public async Task<IActionResult> UploadComplete([FromForm] FileUploadCompleteDTO dto)
		{
			string? filePath;
			try
			{
				filePath = _fileUploadService.MergeChunks(dto.LessonId.ToString(), dto.FileId.ToString(), dto.FileType);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
			if (!string.IsNullOrWhiteSpace(filePath))
			{
				var onProgress = DelegateHelper.CreateS3ProgressDelegate(_hub, dto.FileId);

				_ = Task.Run(async () =>
				{
					try
					{
						await _s3Uploader.Upload(filePath, onProgress, new CancellationToken());
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						UploadHub.TryGetConnection(dto.FileId, out var connectionId);
						if(!string.IsNullOrWhiteSpace(connectionId))
							await _hub.Clients.Client(connectionId).SendAsync("UploadComplete", false);

					}
				});
				return Ok();
			}

			return BadRequest();
		}
	}
}
