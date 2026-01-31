using Amazon;
using Amazon.S3;
using FileUploader.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace FileUploader.Infrastructure.AWS
{
    public class S3ClientFactory : IS3ClientFactory
    {
        private readonly AwsOptions _options;

        public S3ClientFactory(IOptions<AwsOptions> options)
        {
            _options = options.Value;
        }

        public IAmazonS3 Create()
        {
            var region = RegionEndpoint.GetBySystemName(_options.Region);
            return new AmazonS3Client(region);
        }
    }
}
