using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Dispatch;

namespace PrintPlatform.Infrastructure.Data.Configurations
{
    public class JobAssignmentConfiguration : IEntityTypeConfiguration<JobAssignment>
    {
        public void Configure(EntityTypeBuilder<JobAssignment> builder)
        {
            builder.ToTable("JobAssignments", "dispatch");

            builder.HasKey(j => j.Id);
            builder.Property(j => j.RowVersion).IsRowVersion();

            builder.Property(j => j.Status).HasConversion<string>();
            builder.Property(j => j.PayoutStatus).HasConversion<string>();

            builder.HasMany(j => j.QcHistory).WithOne(q => q.JobAssignment).HasForeignKey(q => q.JobAssignmentId);
        }
    }

    public class QCRecordConfiguration : IEntityTypeConfiguration<QCRecord>
    {
        public void Configure(EntityTypeBuilder<QCRecord> builder)
        {
            builder.ToTable("QCRecords", "dispatch");

            builder.HasKey(q => q.Id);
            builder.Property(q => q.Decision).HasConversion<string>();
            builder.Property(q => q.PhotoStorageKeys).HasJsonConversion();
        }
    }
}
