using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Finance;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Finance;

public sealed record RecordCustomerPaymentCommand(
    Guid OrderId,
    decimal AmountEgp,
    string? ExternalReference,
    Guid? CreatedByUserId = null) : IRequest<Result<IReadOnlyList<LedgerEntryDto>>>;

public sealed record InitiateWeeklyPayoutsCommand(
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    PayoutPaymentMethod PaymentMethod = PayoutPaymentMethod.InstaPay) : IRequest<Result<IReadOnlyList<PayoutDto>>>;

public sealed record MarkPayoutSentCommand(
    Guid PayoutId,
    string ExternalReference,
    Guid? CreatedByUserId = null) : IRequest<Result<PayoutDto>>;

public sealed record RecordRefundCommand(
    Guid OrderId,
    decimal AmountEgp,
    string? ExternalReference,
    Guid? CreatedByUserId = null) : IRequest<Result<LedgerEntryDto>>;

public sealed class RecordCustomerPaymentHandler
    : IRequestHandler<RecordCustomerPaymentCommand, Result<IReadOnlyList<LedgerEntryDto>>>
{
    private const decimal DefaultCommissionRate = 0.15m;
    private readonly IFinanceDbContext _db;

    public RecordCustomerPaymentHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<LedgerEntryDto>>> Handle(
        RecordCustomerPaymentCommand request,
        CancellationToken ct)
    {
        if (request.AmountEgp <= 0)
            return FinanceErrors.InvalidAmount;

        await using var tx = await _db.BeginTransactionAsync(ct);

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        var customerUserId = order?.CustomerUserId;
        var orderItemIds = order?.Items.Select(i => i.Id).ToArray() ?? [];
        var assignedPayout = orderItemIds.Length == 0
            ? 0m
            : await _db.JobAssignments
                .Where(j => orderItemIds.Contains(j.OrderItemId))
                .SumAsync(j => j.PayoutAmount, ct);

        var commission = assignedPayout > 0
            ? Math.Max(0m, request.AmountEgp - assignedPayout)
            : request.AmountEgp * DefaultCommissionRate;

        var payment = LedgerEntry.Create(
            LedgerEntryType.CustomerPayment,
            request.AmountEgp,
            LedgerDirection.In,
            request.OrderId,
            null,
            null,
            null,
            customerUserId,
            "Customer payment captured.",
            request.ExternalReference,
            request.CreatedByUserId);

        if (payment.IsFailure)
            return payment.Error;

        var entries = new List<LedgerEntry> { payment.Value };

        if (commission > 0)
        {
            var platformCommission = LedgerEntry.Create(
                LedgerEntryType.PlatformCommission,
                commission,
                LedgerDirection.In,
                request.OrderId,
                null,
                null,
                null,
                customerUserId,
                "Platform commission derived from customer payment.",
                request.ExternalReference,
                request.CreatedByUserId);

            if (platformCommission.IsFailure)
                return platformCommission.Error;

            entries.Add(platformCommission.Value);
        }

        await _db.LedgerEntries.AddRangeAsync(entries, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return entries.Select(e => e.ToDto()).ToArray();
    }
}

public sealed class InitiateWeeklyPayoutsHandler
    : IRequestHandler<InitiateWeeklyPayoutsCommand, Result<IReadOnlyList<PayoutDto>>>
{
    private readonly IFinanceDbContext _db;

    public InitiateWeeklyPayoutsHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<PayoutDto>>> Handle(
        InitiateWeeklyPayoutsCommand request,
        CancellationToken ct)
    {
        if (request.PeriodEnd <= request.PeriodStart)
            return FinanceErrors.InvalidPeriod;

        await using var tx = await _db.BeginTransactionAsync(ct);

        var eligibleJobs = await _db.JobAssignments
            .Where(j => j.Status == JobAssignmentStatus.Delivered
                        && j.PayoutStatus == Domain.Dispatch.PayoutStatus.Pending
                        && j.CompletedAt >= request.PeriodStart
                        && j.CompletedAt < request.PeriodEnd)
            .Select(j => new
            {
                j.Id,
                j.PrinterOwnerUserId,
                j.PayoutAmount
            })
            .ToListAsync(ct);

        if (eligibleJobs.Count == 0)
            return FinanceErrors.EmptyPayoutBatch;

        var payouts = new List<Payout>();
        var ledgerEntries = new List<LedgerEntry>();

        foreach (var group in eligibleJobs.GroupBy(j => j.PrinterOwnerUserId))
        {
            var jobIds = group.Select(j => j.Id).ToArray();
            var net = group.Sum(j => j.PayoutAmount);
            var gross = net;
            var platformFee = 0m;

            var payout = Payout.CreateReadyToSend(
                group.Key,
                request.PeriodStart,
                request.PeriodEnd,
                gross,
                platformFee,
                net,
                request.PaymentMethod,
                jobIds);

            if (payout.IsFailure)
                return payout.Error;

            payouts.Add(payout.Value);

            var ledger = LedgerEntry.Create(
                LedgerEntryType.OwnerPayoutBatched,
                payout.Value.NetAmountEgp,
                LedgerDirection.Out,
                null,
                null,
                payout.Value.Id,
                group.Key,
                null,
                "Owner payout batched for weekly settlement.",
                null,
                null);

            if (ledger.IsFailure)
                return ledger.Error;

            ledgerEntries.Add(ledger.Value);
        }

        await _db.Payouts.AddRangeAsync(payouts, ct);
        await _db.LedgerEntries.AddRangeAsync(ledgerEntries, ct);

        var batchedIds = eligibleJobs.Select(j => j.Id).ToArray();
        await _db.JobAssignments
            .Where(j => batchedIds.Contains(j.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.PayoutStatus, Domain.Dispatch.PayoutStatus.Batched), ct);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return payouts.Select(p => p.ToDto()).ToArray();
    }
}

public sealed class MarkPayoutSentHandler : IRequestHandler<MarkPayoutSentCommand, Result<PayoutDto>>
{
    private readonly IFinanceDbContext _db;

    public MarkPayoutSentHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<PayoutDto>> Handle(MarkPayoutSentCommand request, CancellationToken ct)
    {
        await using var tx = await _db.BeginTransactionAsync(ct);

        var payout = await _db.Payouts.FirstOrDefaultAsync(p => p.Id == request.PayoutId, ct);
        if (payout is null)
            return FinanceErrors.PayoutNotFound;

        var markResult = payout.MarkSent(request.ExternalReference);
        if (markResult.IsFailure)
            return markResult.Error;

        var ledger = LedgerEntry.Create(
            LedgerEntryType.OwnerPayoutPaid,
            payout.NetAmountEgp,
            LedgerDirection.Out,
            null,
            null,
            payout.Id,
            payout.PrinterOwnerUserId,
            null,
            "Owner payout sent.",
            request.ExternalReference,
            request.CreatedByUserId);

        if (ledger.IsFailure)
            return ledger.Error;

        await _db.LedgerEntries.AddAsync(ledger.Value, ct);
        await _db.JobAssignments
            .Where(j => payout.JobAssignmentIds.Contains(j.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.PayoutStatus, Domain.Dispatch.PayoutStatus.Paid), ct);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return payout.ToDto();
    }
}

public sealed class RecordRefundHandler : IRequestHandler<RecordRefundCommand, Result<LedgerEntryDto>>
{
    private readonly IFinanceDbContext _db;

    public RecordRefundHandler(IFinanceDbContext db) => _db = db;

    public async Task<Result<LedgerEntryDto>> Handle(RecordRefundCommand request, CancellationToken ct)
    {
        if (request.AmountEgp <= 0)
            return FinanceErrors.InvalidAmount;

        await using var tx = await _db.BeginTransactionAsync(ct);

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        var entry = LedgerEntry.Create(
            LedgerEntryType.Refund,
            request.AmountEgp,
            LedgerDirection.Out,
            request.OrderId,
            null,
            null,
            null,
            order?.CustomerUserId,
            "Customer refund recorded.",
            request.ExternalReference,
            request.CreatedByUserId);

        if (entry.IsFailure)
            return entry.Error;

        await _db.LedgerEntries.AddAsync(entry.Value, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return entry.Value.ToDto();
    }
}
