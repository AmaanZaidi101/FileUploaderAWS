using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PostUploadProcessor;

public class Function
{

	private readonly AmazonDynamoDBClient _ddb = new();
	private readonly IAmazonS3 _s3 = new AmazonS3Client();
	private readonly IAmazonSimpleNotificationService _sns = new AmazonSimpleNotificationServiceClient();
	private static readonly string _tableName =
	   Environment.GetEnvironmentVariable("VIDEO_UPLOADS_TABLE")
	   ?? throw new InvalidOperationException(
		   "VIDEO_UPLOADS_TABLE environment variable is not set");

	private static readonly string _snsTopicArn =
	   Environment.GetEnvironmentVariable("UPLOAD_SNS_TOPIC_ARN")
	   ?? throw new InvalidOperationException("UPLOAD_SNS_TOPIC_ARN not set");

	/// <summary>
	/// A simple function that takes a string and does a ToUpper
	/// </summary>
	/// <param name="input">The event for the Lambda function handler to process.</param>
	/// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
	/// <returns></returns>
	public async Task FunctionHandler(S3Event evt, ILambdaContext context)
	{
		foreach (var record in evt.Records)
		{
			var bucket = record.S3.Bucket.Name;
			var key = record.S3.Object.Key;
			var size = record.S3.Object.Size;

			context.Logger.LogLine($"File Uploaded: {bucket}/{key}");

			var metadata = await _s3.GetObjectMetadataAsync(bucket, key);

			var contentType = metadata.Headers.ContentType ?? "application/octet-stream";

			var fileId = ExtractFileIdFromKey(key);

			if (string.IsNullOrWhiteSpace(fileId))
			{
				context.Logger.LogLine($"Invalid key format: {key}");
				continue;
			}

			try
			{
				context.Logger.LogLine($"Now saving metadata");
				await SaveMetadataAsync(fileId, bucket, key, size, contentType);
			}
			catch (Exception ex)
			{
				context.Logger.LogLine(ex.ToString());
				throw; // optional but recommended while debugging
			}

			try
			{
				context.Logger.LogLine($"Now sending email");
				await PublishEmailNotificationAsync(fileId, bucket, key, size, contentType);
			}
			catch (Exception ex)
			{
				context.Logger.LogLine(ex.ToString());
				throw; // optional but recommended while debugging
			}

		}
	}

	private async Task SaveMetadataAsync(string fileId, string bucket, string key, long size, string contentType)
	{
		var request = new PutItemRequest
		{
			TableName = _tableName,
			Item = new Dictionary<string, AttributeValue>
			{
				["FileId"] = new AttributeValue { S = fileId },
				["Bucket"] = new AttributeValue { S = bucket },
				["S3Key"] = new AttributeValue { S = key },
				["ContentType"] = new AttributeValue { S = contentType },
				["SizeBytes"] = new AttributeValue { N = size.ToString() },
				["UploadedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") },
				["Status"] = new AttributeValue { S = "Uploaded" }
			}
		};

		await _ddb.PutItemAsync(request);
	}

	private async Task PublishEmailNotificationAsync(string fileId, string bucket, string key, long size, string contentType)
	{
		var message = @$"
        🎬 Video Uploaded Successfully
			File ID: {fileId}
			Bucket: {bucket}
			Key: {key}
			Size: {size} bytes
			Content-Type: {contentType}
			Uploaded At: {DateTime.UtcNow:O}
        ";

		var request = new PublishRequest
		{
			TopicArn = _snsTopicArn,
			Subject = "Video Upload Completed",
			Message = message
		};

		await _sns.PublishAsync(request);
	}

	private static string ExtractFileIdFromKey(string key)
	{
		var fileName = Path.GetFileNameWithoutExtension(key);
		return fileName.Split('_')[0]; // GUID
	}


}
