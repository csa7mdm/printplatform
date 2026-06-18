using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Application.Marketplace;

/// <summary>Hand-written projections to keep handlers terse and allocation-light.</summary>
internal static class MarketplaceMappings
{
    public static PrinterDto ToDto(Printer p) => new(
        p.Id,
        p.PrinterOwnerProfileId,
        p.Brand,
        p.Model,
        p.TechnologyType,
        p.BuildVolumeX,
        p.BuildVolumeY,
        p.BuildVolumeZ,
        p.NozzleDiameter,
        p.SupportedMaterialsJson,
        p.MaxLayerHeight,
        p.MinLayerHeight,
        p.AreaDistrict,
        p.AreaCity,
        p.IsDeliverable,
        p.Status,
        p.CertificationLevel,
        p.MaxConcurrentJobs);

    public static MaterialOptionDto ToDto(MaterialOption m) => new(
        m.Id,
        m.MaterialType,
        m.ColorName,
        m.ColorHex,
        m.PricePerGram,
        m.OwnerCostPerGram,
        m.PropertiesJson,
        m.IsActive);
}
