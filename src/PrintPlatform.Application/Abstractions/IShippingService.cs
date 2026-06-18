using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Abstractions;

public class ShipmentDetails
{
    public string TrackingNumber { get; init; } = null!;
    public string ProviderOrderId { get; init; } = null!;
    public string? LabelUrl { get; init; }
}

public interface IShippingService
{
    Task<Result<ShipmentDetails>> CreateShipment(Guid jobAssignmentId, CancellationToken ct = default);
    Task<Result> CancelShipment(Guid jobAssignmentId, string trackingNumber, CancellationToken ct = default);
    Task<Result<string>> GetShipmentStatus(string trackingNumber, CancellationToken ct = default);
}
