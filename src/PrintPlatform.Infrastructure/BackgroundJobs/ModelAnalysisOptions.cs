namespace PrintPlatform.Infrastructure.BackgroundJobs;

/// <summary>Physical constants used to derive weight + print time from mesh volume.</summary>
public sealed class ModelAnalysisOptions
{
    public const string SectionName = "ModelAnalysis";

    /// <summary>Material density in g/cm³. Default ≈ PLA (1.24 g/cm³).</summary>
    public decimal MaterialDensityGramsPerCc { get; set; } = 1.24m;

    /// <summary>
    /// Effective material throughput in grams printed per hour (accounts for infill,
    /// travel moves, and typical FDM speeds). Used as weight ÷ speed = hours.
    /// </summary>
    public decimal PrintSpeedGramsPerHour { get; set; } = 18.0m;

    /// <summary>
    /// Default infill fraction assumed during analysis (0–1). The analysis weight is the
    /// solid-volume weight scaled toward this fraction to approximate a real print.
    /// </summary>
    public decimal AssumedInfillFraction { get; set; } = 0.20m;

    /// <summary>Fraction of volume always solid regardless of infill (walls/top/bottom).</summary>
    public decimal SolidShellFraction { get; set; } = 0.35m;
}
