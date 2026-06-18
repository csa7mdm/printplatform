using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Marketplace;

/// <summary>
/// A physical printer offered on the supply side of the marketplace by a printer owner.
/// Aggregate root owning its <see cref="PrinterMaterial"/> links.
/// </summary>
public sealed class Printer : BaseAggregateRoot<Guid>
{
    private readonly List<PrinterMaterial> _materials = [];

    // EF Core ctor
    private Printer() { }

    private Printer(
        Guid id,
        Guid printerOwnerProfileId,
        string brand,
        string model,
        TechnologyType technologyType,
        int buildVolumeX,
        int buildVolumeY,
        int buildVolumeZ,
        decimal? nozzleDiameter,
        string supportedMaterialsJson,
        decimal maxLayerHeight,
        decimal minLayerHeight,
        string areaDistrict,
        string areaCity,
        bool isDeliverable)
    {
        Id = id;
        PrinterOwnerProfileId = printerOwnerProfileId;
        Brand = brand;
        Model = model;
        TechnologyType = technologyType;
        BuildVolumeX = buildVolumeX;
        BuildVolumeY = buildVolumeY;
        BuildVolumeZ = buildVolumeZ;
        NozzleDiameter = nozzleDiameter;
        SupportedMaterialsJson = supportedMaterialsJson;
        MaxLayerHeight = maxLayerHeight;
        MinLayerHeight = minLayerHeight;
        AreaDistrict = areaDistrict;
        AreaCity = areaCity;
        IsDeliverable = isDeliverable;
        Status = PrinterStatus.PendingVerification;
        CertificationLevel = 0;
        MaxConcurrentJobs = 1;
    }

    public Guid PrinterOwnerProfileId { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public TechnologyType TechnologyType { get; private set; }

    public int BuildVolumeX { get; private set; }
    public int BuildVolumeY { get; private set; }
    public int BuildVolumeZ { get; private set; }

    /// <summary>Nozzle diameter in mm. Null for resin/SLS technologies.</summary>
    public decimal? NozzleDiameter { get; private set; }

    /// <summary>JSON array of supported material types (denormalized for quick filtering).</summary>
    public string SupportedMaterialsJson { get; private set; } = "[]";

    public decimal MaxLayerHeight { get; private set; }
    public decimal MinLayerHeight { get; private set; }

    public string AreaDistrict { get; private set; } = string.Empty;
    public string AreaCity { get; private set; } = string.Empty;
    public bool IsDeliverable { get; private set; }

    public PrinterStatus Status { get; private set; }

    /// <summary>Certification level 0–5; higher means more trusted/capable.</summary>
    public int CertificationLevel { get; private set; }

    /// <summary>Maximum number of jobs this printer can run concurrently (for load scoring).</summary>
    public int MaxConcurrentJobs { get; private set; }

    public IReadOnlyCollection<PrinterMaterial> Materials => _materials.AsReadOnly();

    public static Result<Printer> Register(
        Guid printerOwnerProfileId,
        string brand,
        string model,
        TechnologyType technologyType,
        int buildVolumeX,
        int buildVolumeY,
        int buildVolumeZ,
        decimal? nozzleDiameter,
        string? supportedMaterialsJson,
        decimal maxLayerHeight,
        decimal minLayerHeight,
        string areaDistrict,
        string areaCity,
        bool isDeliverable)
    {
        if (printerOwnerProfileId == Guid.Empty)
            return Error.Validation("Printer.Owner", "Printer owner profile id is required.");
        if (string.IsNullOrWhiteSpace(brand))
            return Error.Validation("Printer.Brand", "Brand is required.");
        if (string.IsNullOrWhiteSpace(model))
            return Error.Validation("Printer.Model", "Model is required.");
        if (buildVolumeX <= 0 || buildVolumeY <= 0 || buildVolumeZ <= 0)
            return Error.Validation("Printer.BuildVolume", "Build volume dimensions must be positive.");
        if (minLayerHeight <= 0 || maxLayerHeight <= 0 || minLayerHeight > maxLayerHeight)
            return Error.Validation("Printer.LayerHeight", "Layer height range is invalid.");
        if (string.IsNullOrWhiteSpace(areaDistrict))
            return Error.Validation("Printer.AreaDistrict", "Area district is required.");
        if (string.IsNullOrWhiteSpace(areaCity))
            return Error.Validation("Printer.AreaCity", "Area city is required.");

        var printer = new Printer(
            Guid.NewGuid(),
            printerOwnerProfileId,
            brand.Trim(),
            model.Trim(),
            technologyType,
            buildVolumeX,
            buildVolumeY,
            buildVolumeZ,
            nozzleDiameter,
            string.IsNullOrWhiteSpace(supportedMaterialsJson) ? "[]" : supportedMaterialsJson,
            maxLayerHeight,
            minLayerHeight,
            areaDistrict.Trim(),
            areaCity.Trim(),
            isDeliverable);

        printer.RaiseDomainEvent(new PrinterRegisteredEvent(printer.Id, printerOwnerProfileId));
        return printer;
    }

    /// <summary>Attaches a material option to this printer.</summary>
    public Result AddMaterial(Guid materialOptionId, PrintQuality maxQuality, bool isDefault)
    {
        if (materialOptionId == Guid.Empty)
            return Result.Failure(Error.Validation("Printer.Material", "Material option id is required."));

        if (_materials.Any(m => m.MaterialOptionId == materialOptionId))
            return Result.Failure(Error.Conflict("Printer.Material", "Material is already attached to this printer."));

        if (isDefault)
            foreach (var existing in _materials)
                existing.SetDefault(false);

        _materials.Add(new PrinterMaterial(Id, materialOptionId, maxQuality, isDefault));
        Touch();
        return Result.Success();
    }

    /// <summary>Transitions PendingVerification → Active and assigns a certification level.</summary>
    public Result Verify(int certificationLevel)
    {
        if (Status == PrinterStatus.Decommissioned)
            return Result.Failure(Error.Conflict("Printer.Verify", "A decommissioned printer cannot be verified."));
        if (certificationLevel is < 0 or > 5)
            return Result.Failure(Error.Validation("Printer.Certification", "Certification level must be between 0 and 5."));

        Status = PrinterStatus.Active;
        CertificationLevel = certificationLevel;
        Touch();

        RaiseDomainEvent(new PrinterVerifiedEvent(Id, PrinterOwnerProfileId, certificationLevel));
        return Result.Success();
    }

    /// <summary>Updates the certification level for an already-known printer.</summary>
    public Result UpdateCertificationScore(int certificationLevel)
    {
        if (certificationLevel is < 0 or > 5)
            return Result.Failure(Error.Validation("Printer.Certification", "Certification level must be between 0 and 5."));

        CertificationLevel = certificationLevel;
        Touch();

        RaiseDomainEvent(new PrinterCertificationUpdatedEvent(Id, certificationLevel));
        return Result.Success();
    }

    /// <summary>Changes the operational availability (Active/Paused) of the printer.</summary>
    public Result UpdateAvailability(PrinterStatus status, int? maxConcurrentJobs = null)
    {
        if (status is not (PrinterStatus.Active or PrinterStatus.Paused or PrinterStatus.Decommissioned))
            return Result.Failure(Error.Validation("Printer.Availability",
                "Availability can only be set to Active, Paused or Decommissioned."));

        if (Status == PrinterStatus.PendingVerification && status == PrinterStatus.Active)
            return Result.Failure(Error.Conflict("Printer.Availability",
                "Printer must be verified before it can be set Active."));

        if (maxConcurrentJobs is { } mcj)
        {
            if (mcj < 1)
                return Result.Failure(Error.Validation("Printer.MaxConcurrent", "Max concurrent jobs must be at least 1."));
            MaxConcurrentJobs = mcj;
        }

        Status = status;
        Touch();

        RaiseDomainEvent(new PrinterAvailabilityChangedEvent(Id, status));
        return Result.Success();
    }
}
