namespace PrintPlatform.Domain.Marketplace;

/// <summary>Printing technology family supported by a printer.</summary>
public enum TechnologyType
{
    FDM,
    MSLA_Resin,
    SLA,
    SLS,
}

/// <summary>Lifecycle status of a printer in the marketplace.</summary>
public enum PrinterStatus
{
    PendingVerification,
    Active,
    Paused,
    Decommissioned,
}

/// <summary>Material families that can be loaded into a printer.</summary>
public enum MaterialType
{
    PLA,
    PETG,
    ABS,
    TPU,
    Nylon,
    PLA_CF,
    Resin_Standard,
    Resin_Dental,
    Resin_Engineering,
}

/// <summary>
/// Quality tier achievable for a given printer + material combination.
/// Roughly maps to layer-height presets (Draft = coarse, Ultra = finest).
/// </summary>
public enum PrintQuality
{
    Draft,
    Standard,
    Fine,
    Ultra,
}
