using FileUploader.Application.Interfaces;
using Microsoft.AspNetCore.StaticFiles;

namespace FileUploader.Application.Services
{
    public class FileUploadService : IFileUploadService
    {
		private readonly string _tempFolder;

        public FileUploadService()
        {
            _tempFolder = @"C:\\Users\\Batman\\Downloads\\Tmp\\";
		}

        public void SaveChunks(string lessonId, string fileId, int chunkIndex, IFormFile chunk)
        {
			if (!Directory.Exists(_tempFolder)) { Directory.CreateDirectory(_tempFolder); }
			var lessonFolder = Path.Combine(_tempFolder, lessonId, fileId);

			if (chunkIndex == 0 && Directory.Exists(lessonFolder))
				Directory.Delete(lessonFolder, true);

			Directory.CreateDirectory(lessonFolder);

			var chunkPath = Path.Combine(lessonFolder, chunkIndex.ToString());
			var fileStream = File.Create(chunkPath);
			chunk.CopyTo(fileStream);
			fileStream.Dispose();
		}

		public string? MergeChunks(string lessonId, string fileId, string fileType)
		{
			if (!Directory.Exists(_tempFolder)) return null;

			var folderPath = Path.Combine(_tempFolder, lessonId, fileId);
			if (!Directory.Exists(folderPath)) return null;

			var chunks = Directory.GetFiles(folderPath);
			if (chunks.Length == 0)
				return null;

			var extension = GetExtension(fileType);
			if (extension == null) return null;

			string mergedFilePath = Path.Combine(_tempFolder, lessonId, $"Merged{extension}");

			var mergedFile = System.IO.File.Create(mergedFilePath);
			mergedFile.Dispose();
			
			bool exception = false;
			foreach (var chunk in chunks)
			{
				if (exception) break;
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
					exception = true;
				}
				finally {
					file.Dispose(); 
				}
			}

			return exception ? null : mergedFilePath;
		}

		public async Task UploadToS3()
		{

		}


		private string? GetExtension(string contentType)
		{
			var provider = new FileExtensionContentTypeProvider();

			var extensions = provider.Mappings.Where(kv => kv.Value.Equals(contentType, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key);
			return extensions.FirstOrDefault(k => contentType.EndsWith(k.Replace(".", "")));
		}
	}
}
