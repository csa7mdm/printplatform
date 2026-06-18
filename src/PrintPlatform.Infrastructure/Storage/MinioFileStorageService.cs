using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using PrintPlatform.Application.Abstractions;

namespace PrintPlatform.Infrastructure.Storage;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;

    public MinioFileStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        var config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = true,
            UseHttp = !_options.UseHttps
        };
        _s3Client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, config);
    }

    public async Task<string> UploadAsync(Stream stream, string key, string contentType, IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = true
        };

        if (metadata != null)
        {
            foreach (var item in metadata)
            {
                putRequest.Metadata.Add(item.Key, item.Value);
            }
        }

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        return key;
    }

    public Task<string> GetPresignedDownloadUrlAsync(string key, int expiresInSeconds = 60, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            Verb = HttpVerb.GET
        };
        return Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public Task<string> GetPresignedUploadUrlAsync(string key, int expiresInSeconds = 30, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            Verb = HttpVerb.PUT
        };
        return Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetObjectMetadataAsync(_options.BucketName, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
