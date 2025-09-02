using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class GapsScheduleConfiguration : IEntityTypeConfiguration<GapsSchedule>
{

    public void Configure(EntityTypeBuilder<GapsSchedule> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("GapsSchedules");

        builder.Property(g => g.Reference).HasMaxLength(200);
        builder.Property(g => g.Remark).HasMaxLength(500);
        builder.Property(g => g.VendorCode).HasMaxLength(100);
        builder.Property(g => g.VendorName).HasMaxLength(200);
        builder.Property(g => g.Status).HasMaxLength(50);

        builder.HasOne(g => g.BulkSchedule).WithMany(b => b.GapsSchedules).HasForeignKey(g => g.BulkScheduleId);
        builder.HasOne(g => g.Payment).WithMany().HasForeignKey(g => g.PaymentId);
        builder.HasOne(g => g.Vendor).WithMany().HasForeignKey(g => g.VendorId);
    }
}
