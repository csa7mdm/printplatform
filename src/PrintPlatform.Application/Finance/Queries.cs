using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Finance;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Finance;

public sealed record GetOwnerEarningsQuery(Guid PrinterOwnerUserId)
    : IRequest<Result<OwnerEarningsDto>>;

public sealed record GetOwnerEarningsBreakdownQuery(
    Guid PrinterOwnerUserId,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IRequest<Result<OwnerEarningsBreakdownDto>>;

public sealed record GetPlatformRevenueQuery(DateTimeOffset? From = null, DateTimeOffset? To = null)
    : IRequest<Result<PlatformRevenueDto>>;

public sealed record GetPendingPayoutsQuery()
    : IRequest<Result<IReadOnlyList<PayoutDto>>>;

public sealed record GetLedgerAuditQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Guid? RelatedOrderId = null,
    Guid? PrinterOwnerUserId = null,
    Guid? CustomerUserId = null) : IRequest<Result<IReadOnlyList<LedgerEntryDto>>>;

public sealed class GetOwnerEarningsHandler : IRequestHandler<GetOwnerEarningsQuery, Result<OwnerEarningsDto>>
{
    private readonly IFinanceDbContext _db;

    public GetOwnerEarningsHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<OwnerEarningsDto>> Handle(GetOwnerEarningsQuery request, CancellationToken ct)
    {
        var payouts = await _db.Payouts
            .Where(p => p.PrinterOwnerUserId == request.PrinterOwnerUserId)
            .ToListAsync(ct);

        var pending = payouts
            .Where(p => p.Status == PayoutStatus.ReadyToSend)
            .Sum(p => p.NetAmountEgp);

        var paid = payouts
            .Where(p => p.Status is PayoutStatus.Sent or PayoutStatus.Reconciled)
            .Sum(p => p.NetAmountEgp);

        var completedJobs = payouts.SelectMany(p => p.JobAssignmentIds).Distinct().Count();

        return new OwnerEarningsDto(request.PrinterOwnerUserId, pending + paid, pending, paid, completedJobs);
    }
}

public sealed class GetOwnerEarningsBreakdownHandler
    : IRequestHandler<GetOwnerEarningsBreakdownQuery, Result<OwnerEarningsBreakdownDto>>
{
    private readonly IFinanceDbContext _db;

    public GetOwnerEarningsBreakdownHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<OwnerEarningsBreakdownDto>> Handle(
        GetOwnerEarningsBreakdownQuery request,
        CancellationToken ct)
    {
        var from = request.From ?? DateTimeOffset.UtcNow.AddMonths(-3);
        var to = request.To ?? DateTimeOffset.UtcNow;

        var payouts = await _db.Payouts
            .Where(p => p.PrinterOwnerUserId == request.PrinterOwnerUserId
                        && p.CreatedAt >= from
                        && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var entries = await _db.LedgerEntries
            .Where(e => e.PrinterOwnerUserId == request.PrinterOwnerUserId
                        && e.CreatedAt >= from
                        && e.CreatedAt <= to)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        return new OwnerEarningsBreakdownDto(
            request.PrinterOwnerUserId,
            from,
            to,
            payouts.Select(p => p.ToDto()).ToArray(),
            entries.Select(e => e.ToDto()).ToArray());
    }
}

public sealed class GetPlatformRevenueHandler : IRequestHandler<GetPlatformRevenueQuery, Result<PlatformRevenueDto>>
{
    private readonly IFinanceDbContext _db;

    public GetPlatformRevenueHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<PlatformRevenueDto>> Handle(GetPlatformRevenueQuery request, CancellationToken ct)
    {
        var from = request.From ?? DateTimeOffset.UtcNow.AddMonths(-1);
        var to = request.To ?? DateTimeOffset.UtcNow;

        var entries = await _db.LedgerEntries
            .Where(e => e.CreatedAt >= from && e.CreatedAt <= to)
            .ToListAsync(ct);

        var payments = entries
            .Where(e => e.EntryType == LedgerEntryType.CustomerPayment)
            .Sum(e => e.AmountEgp);

        var commissions = entries
            .Where(e => e.EntryType == LedgerEntryType.PlatformCommission)
            .Sum(e => e.AmountEgp);

        var refunds = entries
            .Where(e => e.EntryType == LedgerEntryType.Refund)
            .Sum(e => e.AmountEgp);

        return new PlatformRevenueDto(from, to, payments, commissions, refunds, commissions - refunds);
    }
}

public sealed class GetPendingPayoutsHandler
    : IRequestHandler<GetPendingPayoutsQuery, Result<IReadOnlyList<PayoutDto>>>
{
    private readonly IFinanceDbContext _db;

    public GetPendingPayoutsHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<PayoutDto>>> Handle(GetPendingPayoutsQuery request, CancellationToken ct)
    {
        var payouts = await _db.Payouts
            .Where(p => p.Status == PayoutStatus.ReadyToSend)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);

        return payouts.Select(p => p.ToDto()).ToArray();
    }
}

public sealed class GetLedgerAuditHandler
    : IRequestHandler<GetLedgerAuditQuery, Result<IReadOnlyList<LedgerEntryDto>>>
{
    private readonly IFinanceDbContext _db;

    public GetLedgerAuditHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<LedgerEntryDto>>> Handle(GetLedgerAuditQuery request, CancellationToken ct)
    {
        var query = _db.LedgerEntries.AsQueryable();

        if (request.From is not null)
            query = query.Where(e => e.CreatedAt >= request.From);
        if (request.To is not null)
            query = query.Where(e => e.CreatedAt <= request.To);
        if (request.RelatedOrderId is not null)
            query = query.Where(e => e.RelatedOrderId == request.RelatedOrderId);
        if (request.PrinterOwnerUserId is not null)
            query = query.Where(e => e.PrinterOwnerUserId == request.PrinterOwnerUserId);
        if (request.CustomerUserId is not null)
            query = query.Where(e => e.CustomerUserId == request.CustomerUserId);

        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Take(500)
            .ToListAsync(ct);

        return entries.Select(e => e.ToDto()).ToArray();
    }
}
