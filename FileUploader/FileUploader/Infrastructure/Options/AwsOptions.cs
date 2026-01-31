namespace FileUploader.Infrastructure.Options
{
    public sealed class AwsOptions
    {
        public string Region { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
    }
}
