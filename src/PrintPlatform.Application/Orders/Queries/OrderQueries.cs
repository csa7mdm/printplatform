using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Queries;

// --- GetMyOrders ------------------------------------------------------------

/// <summary>Lists order summaries for the calling customer, newest first.</summary>
public sealed record GetMyOrdersQuery(Guid CustomerUserId)
    : IRequest<IReadOnlyList<OrderSummaryDto>>;

public sealed class GetMyOrdersHandler
    : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderSummaryDto>>
{
    private readonly IAppDbContext _db;
    public GetMyOrdersHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(
        GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerUserId == request.CustomerUserId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(
                o.Id, o.Status, o.PaymentStatus, o.PaymentMethod, o.TotalEgp, o.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// --- GetOrderDetails --------------------------------------------------------

/// <summary>Returns the full details of an order owned by the calling customer.</summary>
public sealed record GetOrderDetailsQuery(Guid OrderId, Guid CustomerUserId)
    : IRequest<Result<OrderDetailsDto>>;

public sealed class GetOrderDetailsHandler
    : IRequestHandler<GetOrderDetailsQuery, Result<OrderDetailsDto>>
{
    private readonly IAppDbContext _db;
    public GetOrderDetailsHandler(IAppDbContext db) => _db = db;

    public async Task<Result<OrderDetailsDto>> Handle(
        GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null || order.CustomerUserId != request.CustomerUserId)
            return Result.Failure<OrderDetailsDto>(OrderErrors.OrderNotFound);

        return ModelFileMapper.ToDetails(order);
    }
}

// --- GetPendingQuotes (operator) -------------------------------------------

/// <summary>Lists quote requests awaiting operator pricing, oldest first.</summary>
public sealed record GetPendingQuotesQuery
    : IRequest<IReadOnlyList<PendingQuoteDto>>;

public sealed class GetPendingQuotesHandler
    : IRequestHandler<GetPendingQuotesQuery, IReadOnlyList<PendingQuoteDto>>
{
    private readonly IAppDbContext _db;
    public GetPendingQuotesHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PendingQuoteDto>> Handle(
        GetPendingQuotesQuery request, CancellationToken cancellationToken)
    {
        return await _db.QuoteRequests
            .AsNoTracking()
            .Where(q => q.Status == QuoteRequestStatus.PendingReview)
            .OrderBy(q => q.CreatedAt)
            .Select(q => new PendingQuoteDto(
                q.Id, q.ModelFileId, q.CustomerUserId, q.MaterialOptionId,
                q.QualityPreset, q.Quantity, q.IncludeDesignService, q.Status, q.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// --- GetModelFile -----------------------------------------------------------

/// <summary>Returns a model file (and its analysis result) owned by the caller.</summary>
public sealed record GetModelFileQuery(Guid ModelFileId, Guid CustomerUserId)
    : IRequest<Result<ModelFileDto>>;

public sealed class GetModelFileHandler
    : IRequestHandler<GetModelFileQuery, Result<ModelFileDto>>
{
    private readonly IAppDbContext _db;
    public GetModelFileHandler(IAppDbContext db) => _db = db;

    public async Task<Result<ModelFileDto>> Handle(
        GetModelFileQuery request, CancellationToken cancellationToken)
    {
        var model = await _db.ModelFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.ModelFileId, cancellationToken);

        if (model is null || model.CustomerUserId != request.CustomerUserId)
            return Result.Failure<ModelFileDto>(OrderErrors.ModelFileNotFound);

        return ModelFileMapper.ToDto(model);
    }
}
