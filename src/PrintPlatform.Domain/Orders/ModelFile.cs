using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Orders;

/// <summary>
/// A 3D model file uploaded by a customer. The binary is stored in object storage
/// (S3 / MinIO) — never in the database; only the <see cref="StorageKey"/> is persisted.
/// Geometry analysis populates the estimated volume / weight / print-time fields.
/// </summary>
public sealed class ModelFile : BaseAggregateRoot<Guid>
{
    private ModelFile() { } // EF

    private ModelFile(
        Guid id,
        Guid customerUserId,
        string storageKey,
        string originalFileName,
        long fileSizeBytes,
        FileFormat fileFormat)
    {
        Id = id;
        CustomerUserId = customerUserId;
        StorageKey = storageKey;
        OriginalFileName = originalFileName;
        FileSizeBytes = fileSizeBytes;
        FileFormat = fileFormat;
        Status = ModelFileStatus.Uploaded;
    }

    public Guid CustomerUserId { get; private set; }

    /// <summary>Object-storage key (S3/MinIO). The binary itself is never stored in the DB.</summary>
    public string StorageKey { get; private set; } = default!;

    public string OriginalFileName { get; private set; } = default!;
    public long FileSizeBytes { get; private set; }
    public FileFormat FileFormat { get; private set; }
    public ModelFileStatus Status { get; private set; }

    // --- Populated by geometry analysis -------------------------------------
    public decimal? EstimatedVolumeCC { get; private set; }
    public decimal? BoundingBoxX { get; private set; }
    public decimal? BoundingBoxY { get; private set; }
    public decimal? BoundingBoxZ { get; private set; }
    public decimal? EstimatedWeightGrams { get; private set; }
    public decimal? EstimatedPrintHours { get; private set; }
    public string? AnalysisNotes { get; private set; }

    /// <summary>
    /// Creates a new uploaded model file and raises <see cref="ModelFileUploadedEvent"/>.
    /// </summary>
    public static ModelFile Create(
        Guid customerUserId,
        string storageKey,
        string originalFileName,
        long fileSizeBytes,
        FileFormat fileFormat)
    {
        var file = new ModelFile(Guid.NewGuid(), customerUserId, storageKey, originalFileName, fileSizeBytes, fileFormat);
        file.RaiseDomainEvent(new ModelFileUploadedEvent(
            file.Id, customerUserId, storageKey, fileFormat, DateTimeOffset.UtcNow));
        return file;
    }

    /// <summary>Marks the file as queued for geometry analysis.</summary>
    public void MarkAnalysisPending()
    {
        Status = ModelFileStatus.AnalysisPending;
        Touch();
    }

    /// <summary>
    /// Records the computed geometry, transitions to <see cref="ModelFileStatus.Approved"/>
    /// and raises <see cref="ModelFileAnalysedEvent"/>.
    /// </summary>
    public void ApplyAnalysis(
        decimal volumeCc,
        decimal boundingBoxX,
        decimal boundingBoxY,
        decimal boundingBoxZ,
        decimal weightGrams,
        decimal printHours,
        string? notes = null)
    {
        EstimatedVolumeCC = volumeCc;
        BoundingBoxX = boundingBoxX;
        BoundingBoxY = boundingBoxY;
        BoundingBoxZ = boundingBoxZ;
        EstimatedWeightGrams = weightGrams;
        EstimatedPrintHours = printHours;
        AnalysisNotes = notes;
        Status = ModelFileStatus.Approved;
        Touch();

        RaiseDomainEvent(new ModelFileAnalysedEvent(
            Id, volumeCc, weightGrams, printHours, DateTimeOffset.UtcNow));
    }

    /// <summary>Marks the file as rejected (e.g. unreadable mesh) with a reason.</summary>
    public void Reject(string reason)
    {
        Status = ModelFileStatus.Rejected;
        AnalysisNotes = reason;
        Touch();
    }
}
