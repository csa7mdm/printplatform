using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Application.Marketplace;

/// <summary>
/// Pure ranking logic for matching printers to a job.
/// <para>
/// score = capabilityMatch*40 + proximityScore*30 + certificationScore*20 + loadScore*10
/// </para>
/// </summary>
public sealed class PrinterRankingService
{
    private const double MaxRankableDistanceKm = 30d;
    private const double MaxCertificationLevel = 5d;

    private readonly IDistrictDistanceCalculator _distance;

    public PrinterRankingService(IDistrictDistanceCalculator distance) => _distance = distance;

    /// <summary>
    /// Scores a single printer against a job. Returns null when the printer fails the
    /// hard capability filter (material not supported or build volume too small).
    /// </summary>
    public RankedPrinterDto? Score(
        Printer printer,
        IReadOnlyCollection<MaterialType> printerMaterials,
        JobRequirements job,
        int activeJobs)
    {
        var capabilityMatch = ComputeCapabilityMatch(printer, printerMaterials, job);
        if (capabilityMatch <= 0d)
            return null; // hard filter — excluded entirely

        var distanceKm = _distance.GetDistanceKm(printer.AreaDistrict, job.CustomerDistrict);
        var proximityScore = Clamp01(1d - distanceKm / MaxRankableDistanceKm);

        var certificationScore = Clamp01(printer.CertificationLevel / MaxCertificationLevel);

        var maxConcurrent = Math.Max(1, printer.MaxConcurrentJobs);
        var loadScore = Clamp01(1d - (double)activeJobs / maxConcurrent);

        var score = capabilityMatch * 40d
                  + proximityScore * 30d
                  + certificationScore * 20d
                  + loadScore * 10d;

        return new RankedPrinterDto(
            ToDto(printer),
            Math.Round(score, 4),
            capabilityMatch,
            Math.Round(proximityScore, 4),
            Math.Round(certificationScore, 4),
            Math.Round(loadScore, 4),
            Math.Round(distanceKm, 2));
    }

    /// <summary>Hard filter: 1 when material supported AND build volume fits, otherwise 0.</summary>
    private static double ComputeCapabilityMatch(
        Printer printer,
        IReadOnlyCollection<MaterialType> printerMaterials,
        JobRequirements job)
    {
        var materialOk = printerMaterials.Contains(job.RequiredMaterial);

        var volumeOk = printer.BuildVolumeX >= job.RequiredVolumeX
                    && printer.BuildVolumeY >= job.RequiredVolumeY
                    && printer.BuildVolumeZ >= job.RequiredVolumeZ;

        return materialOk && volumeOk ? 1d : 0d;
    }

    private static double Clamp01(double value) => Math.Clamp(value, 0d, 1d);

    private static PrinterDto ToDto(Printer p) => new(
        p.Id, p.PrinterOwnerProfileId, p.Brand, p.Model, p.TechnologyType,
        p.BuildVolumeX, p.BuildVolumeY, p.BuildVolumeZ, p.NozzleDiameter,
        p.SupportedMaterialsJson, p.MaxLayerHeight, p.MinLayerHeight,
        p.AreaDistrict, p.AreaCity, p.IsDeliverable, p.Status,
        p.CertificationLevel, p.MaxConcurrentJobs);
}
