using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("Payments");

        builder.Property(p => p.InvoiceNumber).HasMaxLength(100);
        builder.Property(p => p.GrossAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.VatAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.WhtAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NetAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Status).HasMaxLength(50);
        builder.Property(p => p.Reference).HasMaxLength(200);

        // Partial payment fields
        builder.Property(p => p.OriginalInvoiceAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PaymentAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalAmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(p => p.IsPartialPayment).HasDefaultValue(false);
        builder.Property(p => p.IsFinalPayment).HasDefaultValue(false);
        builder.Property(p => p.ParentPaymentId).HasMaxLength(50);

        builder.HasOne(p => p.Vendor).WithMany(v => v.Payments).HasForeignKey(p => p.VendorId);
        builder.HasOne(p => p.BulkSchedule).WithMany(b => b.Payments).HasForeignKey(p => p.BulkScheduleId)
            .IsRequired(false);
        
        // Configure partial payment relationships
        builder.HasOne(p => p.ParentPayment)
            .WithMany(p => p.ChildPayments)
            .HasForeignKey(p => p.ParentPaymentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // explicitly configure user relations to avoid ambiguity when multiple navigations to User exist
        builder.HasOne(p => p.CreatedByUser)
            .WithMany(u => u.CreatedPayments)
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ApprovedByUser)
            .WithMany() // no inverse navigation defined on User
            .HasForeignKey(p => p.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
