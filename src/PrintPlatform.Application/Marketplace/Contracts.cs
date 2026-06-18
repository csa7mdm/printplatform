using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Application.Marketplace;

// ---------------------------------------------------------------------------
// Read DTOs
// ---------------------------------------------------------------------------

/// <summary>Projection of a <see cref="Printer"/> for owner/listing views.</summary>
public sealed record PrinterDto(
    Guid Id,
    Guid PrinterOwnerProfileId,
    string Brand,
    string Model,
    TechnologyType TechnologyType,
    int BuildVolumeX,
    int BuildVolumeY,
    int BuildVolumeZ,
    decimal? NozzleDiameter,
    string SupportedMaterialsJson,
    decimal MaxLayerHeight,
    decimal MinLayerHeight,
    string AreaDistrict,
    string AreaCity,
    bool IsDeliverable,
    PrinterStatus Status,
    int CertificationLevel,
    int MaxConcurrentJobs);

/// <summary>Projection of a <see cref="MaterialOption"/>.</summary>
public sealed record MaterialOptionDto(
    Guid Id,
    MaterialType MaterialType,
    string ColorName,
    string ColorHex,
    decimal PricePerGram,
    decimal OwnerCostPerGram,
    string PropertiesJson,
    bool IsActive);

/// <summary>A ranked printer candidate for a job, with score breakdown.</summary>
public sealed record RankedPrinterDto(
    PrinterDto Printer,
    double Score,
    double CapabilityMatch,
    double ProximityScore,
    double CertificationScore,
    double LoadScore,
    double DistanceKm);

// ---------------------------------------------------------------------------
// External-context abstractions (resolved by Infrastructure)
// ---------------------------------------------------------------------------

/// <summary>The supply-side requirements of a print job, used for ranking.</summary>
public sealed record JobRequirements(
    Guid JobId,
    MaterialType RequiredMaterial,
    int RequiredVolumeX,
    int RequiredVolumeY,
    int RequiredVolumeZ,
    string CustomerDistrict,
    string CustomerCity);

/// <summary>Resolves the supply-side requirements for a given job id.</summary>
public interface IJobRequirementsProvider
{
    Task<JobRequirements?> GetAsync(Guid jobId, CancellationToken cancellationToken = default);
}

/// <summary>Provides the current active job count for a printer (load scoring).</summary>
public interface IPrinterLoadProvider
{
    Task<IReadOnlyDictionary<Guid, int>> GetActiveJobCountsAsync(
        IEnumerable<Guid> printerIds,
        CancellationToken cancellationToken = default);
}

/// <summary>Approximate driving distance between two Cairo/Giza districts (km).</summary>
public interface IDistrictDistanceCalculator
{
    double GetDistanceKm(string fromDistrict, string toDistrict);
}
