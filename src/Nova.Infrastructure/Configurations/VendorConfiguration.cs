using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("Vendors");

        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Code).HasMaxLength(50);
        builder.Property(v => v.AccountName).HasMaxLength(200);
        builder.Property(v => v.AccountNumber).HasMaxLength(50);
        builder.Property(v => v.Address).HasMaxLength(500);
        builder.Property(v => v.PhoneNumber).HasMaxLength(50);
        builder.Property(v => v.Email).HasMaxLength(256);
        builder.Property(v => v.TaxIdentificationNumber).HasMaxLength(100);
        builder.Property(v => v.TaxType).HasMaxLength(20).HasDefaultValue("Both");
        builder.Property(v => v.VatRate).HasPrecision(5, 2).HasDefaultValue(7.5m);
        builder.Property(v => v.WhtRate).HasPrecision(5, 2).HasDefaultValue(2.0m);
        builder.Property(v => v.IsActive).HasDefaultValue(true);
        builder.Property(v => v.TenantId).HasMaxLength(450).IsRequired();

        // Relationships
        builder.HasOne(v => v.Bank)
            .WithMany(b => b.Vendors)
            .HasForeignKey(v => v.BankId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.Tenant)
            .WithMany(t => t.Vendors)
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(v => new { v.TenantId, v.Name }).IsUnique();
        builder.HasIndex(v => v.TenantId);
        builder.HasIndex(v => v.Code);
    }
}
