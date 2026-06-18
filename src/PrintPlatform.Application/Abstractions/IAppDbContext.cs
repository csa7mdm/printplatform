using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Application.Abstractions;

/// <summary>
/// Application-facing view of the persistence context. Exposes only the aggregate
/// sets the Orders + Quoting module needs, keeping handlers decoupled from the
/// concrete EF Core DbContext in Infrastructure.
/// </summary>
public interface IAppDbContext
{
    DbSet<ModelFile> ModelFiles { get; }
    DbSet<QuoteRequest> QuoteRequests { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<JobAssignment> JobAssignments { get; }
    DbSet<QCRecord> QCRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
