namespace FileUploader.Infrastructure.AWS
{
    public static class S3ObjectKeyBuilder
    {
        public static string Build(string originalFileName)
        {
            var safeName = Path.GetFileName(originalFileName);
            return $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{safeName}";
        }
    }
}
