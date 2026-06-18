using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Cancels an order on behalf of its owning customer if it has not yet shipped.
/// Raises <see cref="OrderCancelledEvent"/> for downstream refund / notification handling.
/// </summary>
public sealed record CancelOrderCommand(
    Guid OrderId,
    Guid CustomerUserId,
    string Reason) : IRequest<Result>;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IAppDbContext _db;

    public CancelOrderHandler(IAppDbContext db) => _db = db;

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(OrderErrors.OrderNotFound);

        if (order.CustomerUserId != request.CustomerUserId)
            return Result.Failure(OrderErrors.OrderNotFound);

        var result = order.Cancel(string.IsNullOrWhiteSpace(request.Reason) ? "Cancelled by customer." : request.Reason);
        if (result.IsFailure)
            return result;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
