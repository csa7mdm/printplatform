using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;
using PrintPlatform.Infrastructure.Identity;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// Primary application DbContext. Hosts both the ASP.NET Identity credential tables
/// and the Identity domain aggregates (<see cref="User"/> and profiles). Dispatches
/// domain events after a successful commit and applies a global soft-delete filter.
/// </summary>
public sealed class AppDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IAppDbContext
{
    private readonly IDomainEventDispatcher? _dispatcher;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDomainEventDispatcher? dispatcher = null)
        : base(options)
        => _dispatcher = dispatcher;

    public DbSet<User> Users => Set<User>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<PrinterOwnerProfile> PrinterOwnerProfiles => Set<PrinterOwnerProfile>();

    // -- Orders + Quoting module ------------------------------------------------
    public DbSet<ModelFile> ModelFiles => Set<ModelFile>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // -- Dispatch module ------------------------------------------------------
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
    public DbSet<QCRecord> QCRecords => Set<QCRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Keep ASP.NET Identity tables on a dedicated schema, domain on "identity".
        builder.HasDefaultSchema("identity");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect aggregates with pending events BEFORE the commit.
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_dispatcher is not null && aggregates.Count > 0)
            await _dispatcher.DispatchAndClearAsync(aggregates, cancellationToken);

        return result;
    }
}
