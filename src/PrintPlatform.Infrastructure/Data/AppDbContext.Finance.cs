using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PrintPlatform.Application.Finance;
using PrintPlatform.Domain.Finance;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// Finance module DbSets and transaction surface. Lives in a partial file so the
/// finance slice owns its persistence shape without editing the core DbContext.
/// </summary>
public sealed partial class AppDbContext : IFinanceDbContext
{
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Payout> Payouts => Set<Payout>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        Database.BeginTransactionAsync(ct);
}
