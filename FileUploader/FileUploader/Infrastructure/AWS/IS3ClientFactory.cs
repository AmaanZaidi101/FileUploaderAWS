using Amazon.S3;

namespace FileUploader.Infrastructure.AWS
{
    public interface IS3ClientFactory
    {
        IAmazonS3 Create();
    }
}
