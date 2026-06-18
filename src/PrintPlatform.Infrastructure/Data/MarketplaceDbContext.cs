using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Marketplace;
using PrintPlatform.Domain.Marketplace;
using PrintPlatform.Domain.Shared;
using PrintPlatform.Infrastructure.Data.Configurations;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// EF Core context for the Marketplace (supply-side) module.
/// Tables are prefixed <c>mkt_</c>. Dispatches domain events after commit.
/// </summary>
public sealed class MarketplaceDbContext : DbContext, IMarketplaceDbContext
{
    private readonly IDomainEventDispatcher? _dispatcher;

    public MarketplaceDbContext(
        DbContextOptions<MarketplaceDbContext> options,
        IDomainEventDispatcher? dispatcher = null)
        : base(options)
        => _dispatcher = dispatcher;

    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<PrinterMaterial> PrinterMaterials => Set<PrinterMaterial>();
    public DbSet<MaterialOption> MaterialOptions => Set<MaterialOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PrinterConfiguration());
        modelBuilder.ApplyConfiguration(new MaterialOptionConfiguration());
        modelBuilder.ApplyConfiguration(new PrinterMaterialConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture aggregates with pending events before saving.
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var affected = await base.SaveChangesAsync(cancellationToken);

        if (_dispatcher is not null && aggregates.Count > 0)
            await _dispatcher.DispatchAndClearAsync(aggregates, cancellationToken);

        return affected;
    }
}
