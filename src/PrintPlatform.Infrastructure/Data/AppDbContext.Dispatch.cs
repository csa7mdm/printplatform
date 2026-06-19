using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Dispatch;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// Dispatch module DbSets. Lives in its own partial file so the module owns its
/// persistence surface without editing the shared AppDbContext core.
/// </summary>
public sealed partial class AppDbContext
{
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
    public DbSet<QCRecord> QCRecords => Set<QCRecord>();
}
