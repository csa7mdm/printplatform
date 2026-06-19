using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PrintPlatform.Domain.Finance;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Application.Finance;

public interface IFinanceDbContext
{
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<Payout> Payouts { get; }
    DbSet<PrintPlatform.Domain.Dispatch.JobAssignment> JobAssignments { get; }
    DbSet<Order> Orders { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record LedgerEntryDto(
    Guid Id,
    LedgerEntryType EntryType,
    decimal AmountEgp,
    LedgerDirection Direction,
    Guid? RelatedOrderId,
    Guid? RelatedJobAssignmentId,
    Guid? RelatedPayoutId,
    Guid? PrinterOwnerUserId,
    Guid? CustomerUserId,
    string Description,
    string? ExternalReference,
    DateTimeOffset CreatedAt,
    Guid? CreatedByUserId);

public sealed record PayoutDto(
    Guid Id,
    Guid PrinterOwnerUserId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal GrossAmountEgp,
    decimal PlatformFeeEgp,
    decimal NetAmountEgp,
    PayoutStatus Status,
    PayoutPaymentMethod PaymentMethod,
    string? ExternalReference,
    IReadOnlyList<Guid> JobAssignmentIds,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? ReconciledAt);

public sealed record OwnerEarningsDto(
    Guid PrinterOwnerUserId,
    decimal TotalEarnedEgp,
    decimal PendingPayoutEgp,
    decimal PaidEgp,
    int CompletedJobs);

public sealed record OwnerEarningsBreakdownDto(
    Guid PrinterOwnerUserId,
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyList<PayoutDto> Payouts,
    IReadOnlyList<LedgerEntryDto> LedgerEntries);

public sealed record PlatformRevenueDto(
    DateTimeOffset From,
    DateTimeOffset To,
    decimal CustomerPaymentsEgp,
    decimal PlatformCommissionEgp,
    decimal RefundsEgp,
    decimal NetRevenueEgp);

internal static class FinanceMappings
{
    public static LedgerEntryDto ToDto(this LedgerEntry entry) =>
        new(
            entry.Id,
            entry.EntryType,
            entry.AmountEgp,
            entry.Direction,
            entry.RelatedOrderId,
            entry.RelatedJobAssignmentId,
            entry.RelatedPayoutId,
            entry.PrinterOwnerUserId,
            entry.CustomerUserId,
            entry.Description,
            entry.ExternalReference,
            entry.CreatedAt,
            entry.CreatedByUserId);

    public static PayoutDto ToDto(this Payout payout) =>
        new(
            payout.Id,
            payout.PrinterOwnerUserId,
            payout.PeriodStart,
            payout.PeriodEnd,
            payout.GrossAmountEgp,
            payout.PlatformFeeEgp,
            payout.NetAmountEgp,
            payout.Status,
            payout.PaymentMethod,
            payout.ExternalReference,
            payout.JobAssignmentIds,
            payout.CreatedAt,
            payout.SentAt,
            payout.ReconciledAt);
}
