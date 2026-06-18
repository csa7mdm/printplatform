using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Marketplace;

/// <summary>
/// A catalog entry describing a specific material + color a printer owner offers,
/// together with its retail (per-gram) pricing and owner cost basis.
/// </summary>
public sealed class MaterialOption : BaseAggregateRoot<Guid>
{
    // EF Core ctor
    private MaterialOption() { }

    private MaterialOption(
        Guid id,
        MaterialType materialType,
        string colorName,
        string colorHex,
        decimal pricePerGram,
        decimal ownerCostPerGram,
        string propertiesJson)
    {
        Id = id;
        MaterialType = materialType;
        ColorName = colorName;
        ColorHex = colorHex;
        PricePerGram = pricePerGram;
        OwnerCostPerGram = ownerCostPerGram;
        PropertiesJson = propertiesJson;
        IsActive = true;
    }

    public MaterialType MaterialType { get; private set; }

    /// <summary>Human-readable color name, e.g. "Galaxy Black".</summary>
    public string ColorName { get; private set; } = string.Empty;

    /// <summary>Hex color code, e.g. "#1A1A1A".</summary>
    public string ColorHex { get; private set; } = string.Empty;

    /// <summary>Retail price charged to customers per gram (EGP).</summary>
    public decimal PricePerGram { get; private set; }

    /// <summary>Owner's cost basis per gram (EGP) — used for margin reporting.</summary>
    public decimal OwnerCostPerGram { get; private set; }

    /// <summary>JSON blob of property flags (food-safe, flexible, uv-resistant, etc.).</summary>
    public string PropertiesJson { get; private set; } = "{}";

    public bool IsActive { get; private set; }

    public static Result<MaterialOption> Create(
        MaterialType materialType,
        string colorName,
        string colorHex,
        decimal pricePerGram,
        decimal ownerCostPerGram,
        string? propertiesJson = null)
    {
        if (string.IsNullOrWhiteSpace(colorName))
            return Error.Validation("MaterialOption.ColorName", "Color name is required.");

        if (pricePerGram <= 0)
            return Error.Validation("MaterialOption.PricePerGram", "Price per gram must be positive.");

        if (ownerCostPerGram < 0)
            return Error.Validation("MaterialOption.OwnerCost", "Owner cost cannot be negative.");

        return new MaterialOption(
            Guid.NewGuid(),
            materialType,
            colorName.Trim(),
            (colorHex ?? string.Empty).Trim(),
            pricePerGram,
            ownerCostPerGram,
            string.IsNullOrWhiteSpace(propertiesJson) ? "{}" : propertiesJson);
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}
