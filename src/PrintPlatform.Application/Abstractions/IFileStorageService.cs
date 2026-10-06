namespace PrintPlatform.Application.Abstractions;

/// <summary>
/// Defines the contract for a transactional file storage service,
/// typically interacting with an object storage like S3 or MinIO.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream stream,
        string key,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedDownloadUrlAsync(
        string key,
        int expiresInSeconds = 60,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedUploadUrlAsync(
        string key,
        int expiresInSeconds = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads an object's content as a stream. The caller is responsible for
    /// disposing the returned stream.
    /// </summary>
    Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}