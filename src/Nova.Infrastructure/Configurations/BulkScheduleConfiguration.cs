using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class BulkScheduleConfiguration : IEntityTypeConfiguration<BulkSchedule>
{
    public void Configure(EntityTypeBuilder<BulkSchedule> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("BulkSchedules");

        builder.Property(b => b.BatchNumber).HasMaxLength(100);
        builder.Property(b => b.Description).HasMaxLength(500);
        builder.Property(b => b.TotalGrossAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalVatAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalWhtAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.TotalNetAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Status).HasMaxLength(50);

        // Configure tenant relationship
        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Payments).WithOne(p => p.BulkSchedule).HasForeignKey(p => p.BulkScheduleId);
        builder.HasMany(b => b.GapsSchedules).WithOne(g => g.BulkSchedule).HasForeignKey(g => g.BulkScheduleId);
    }
}
