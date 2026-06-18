using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Dispatch;

namespace PrintPlatform.Application.Dispatch;

public interface IDispatchDbContext
{
    DbSet<JobAssignment> JobAssignments { get; }
    DbSet<QCRecord> QCRecords { get; }
}
