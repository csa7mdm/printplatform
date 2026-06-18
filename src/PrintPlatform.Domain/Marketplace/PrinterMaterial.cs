using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Marketplace;

/// <summary>
/// Join entity linking a <see cref="Printer"/> to a <see cref="MaterialOption"/>,
/// capturing the maximum quality achievable for that combination and whether it is
/// the printer's default material.
/// </summary>
public sealed class PrinterMaterial : BaseEntity<Guid>
{
    private PrinterMaterial() { }

    internal PrinterMaterial(Guid printerId, Guid materialOptionId, PrintQuality maxQuality, bool isDefault)
    {
        Id = Guid.NewGuid();
        PrinterId = printerId;
        MaterialOptionId = materialOptionId;
        MaxQuality = maxQuality;
        IsDefault = isDefault;
    }

    public Guid PrinterId { get; private set; }
    public Guid MaterialOptionId { get; private set; }
    public PrintQuality MaxQuality { get; private set; }
    public bool IsDefault { get; private set; }

    public MaterialOption? MaterialOption { get; private set; }

    internal void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        Touch();
    }
}
