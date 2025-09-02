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
        builder.Property(v => v.AccountNumber).HasMaxLength(50);
        builder.Property(v => v.TaxIdentificationNumber).HasMaxLength(100);

        builder.HasOne(v => v.Bank).WithMany(b => b.Vendors).HasForeignKey(v => v.BankId);
    }
}
