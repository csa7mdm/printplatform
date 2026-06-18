namespace PrintPlatform.Application.Abstractions;

/// <summary>
/// Defines the contract for a transactional file storage service,
/// typically interacting with an object storage like S3 or MinIO.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file stream to the storage provider.
    /// </summary>
    /// <param name="stream">The file content stream.</param>
    /// <param name="key">The unique object key (path) for the file.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="metadata">Optional metadata to store with the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique key of the stored object.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string key,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary, pre-signed URL to allow a client to download a file directly.
    /// </summary>
    /// <param name="key">The key of the object to download.</param>
    /// <param name="expiresInSeconds">The duration the URL is valid for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A pre-signed URL string.</returns>
    Task<string> GetPresignedDownloadUrlAsync(
        string key,
        int expiresInSeconds = 60,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary, pre-signed URL to allow a client to upload a file directly.
    /// </summary>
    /// <param name="key">The key of the object to upload to.</param>
    /// <param name="expiresInSeconds">The duration the URL is valid for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A pre-signed URL string.</returns>
    Task<string> GetPresignedUploadUrlAsync(
        string key,
        int expiresInSeconds = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an object from the storage provider.
    /// </summary>
    /// <param name="key">The key of the object to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an object exists in the storage provider.
    /// </summary>
    /// <param name="key">The key of the object to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the object exists, otherwise false.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
