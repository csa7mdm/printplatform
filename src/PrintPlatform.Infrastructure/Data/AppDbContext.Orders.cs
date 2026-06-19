using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// Orders + Quoting module DbSets. Lives in its own partial file so the module
/// owns its persistence surface without editing the shared AppDbContext core.
/// </summary>
public sealed partial class AppDbContext
{
    public DbSet<ModelFile> ModelFiles => Set<ModelFile>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
