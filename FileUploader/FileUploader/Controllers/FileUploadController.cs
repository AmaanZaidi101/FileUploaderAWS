using System.Threading.Tasks;
using FileUploader.Models;
using FileUploader.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;

namespace FileUploader.Controllers
{
	[ApiController]
	[Route("/api/[controller]")]
	public class FileUploadController : ControllerBase
	{
		private readonly string _tempFolder;
		private readonly int _chunkSize;
		private readonly int _maxSize;
        private readonly IHubContext<UploadHub> _hub;
        public FileUploadController(IHubContext<UploadHub> hub)
        {
            _tempFolder = @"C:\\Users\\Batman\\Downloads\\Tmp\\";
            _chunkSize = 500 * 1024;
            _maxSize = 10000 * 1024;
            _hub = hub;
        }
        [HttpPost("chunk")]
		public async Task<IActionResult> UploadChunk([FromForm] FileUploadDTO file)
		{
			if (!Directory.Exists(_tempFolder)) { Directory.CreateDirectory(_tempFolder); }
			var lessonFolder = Path.Combine(_tempFolder, file.LessonId.ToString(), file.FileId.ToString());

			if (file.ChunkIndex == 0 && Directory.Exists(lessonFolder))
				Directory.Delete(lessonFolder, true);

			Directory.CreateDirectory(lessonFolder);

			var chunkPath = Path.Combine(lessonFolder, file.ChunkIndex.ToString());
			var fileStream = System.IO.File.Create(chunkPath);
			file.Chunk.CopyTo(fileStream);
			fileStream.Dispose();

			return Ok();
		}

		[HttpPost("complete")]
		public async Task<IActionResult> UploadComplete([FromForm] FileUploadCompleteDTO dto)
		{
			if (!Directory.Exists(_tempFolder)) return StatusCode(500);

			var folderPath = Path.Combine(_tempFolder, dto.LessonId.ToString(), dto.FileId.ToString());
			if (!Directory.Exists(folderPath)) return NotFound();

			var chunks = Directory.GetFiles(folderPath);
			if (chunks.Length == 0)
				return NotFound();

			var extension = GetExtension(dto.FileType);
			if (extension == null) return BadRequest();

			string mergedFilePath = Path.Combine(_tempFolder, dto.LessonId.ToString(), $"Merged{extension}");

			var mergedFile = System.IO.File.Create(mergedFilePath);
			mergedFile.Dispose();
			foreach (var chunk in chunks)
			{
				using var file = System.IO.File.Open(chunk, FileMode.Open);
				byte[] fileBytes = new byte[file.Length];
				try
				{
					file.ReadExactly(fileBytes);
					System.IO.File.AppendAllBytes(mergedFilePath, fileBytes);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				finally { file.Dispose(); }
			}
			return Ok();
		}

		[HttpPost("progress")]
        public async Task CommunicateS3Progess([FromBody]Guid fileId)
        {
			if (!UploadHub.TryGetConnection(fileId, out var connectionId)) return;

			await _hub.Clients.Client(connectionId).SendAsync("UploadProgress", "Initialising");

			for (int i = 0; i <= 100; i+=10)
			{
				await Task.Delay(500);
				await _hub.Clients.Client(connectionId).SendAsync("UploadProgress", $"{i} %");
			}
			await Task.Delay(500);
			await _hub.Clients.Client(connectionId).SendAsync("UploadComplete", true);
        }

        private string? GetExtension(string contentType)
		{
			var provider = new FileExtensionContentTypeProvider();

			var  extensions = provider.Mappings.Where(kv => kv.Value.Equals(contentType, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key);
			return extensions.FirstOrDefault(k => contentType.EndsWith(k.Replace(".", "")));
		}
	}
}
