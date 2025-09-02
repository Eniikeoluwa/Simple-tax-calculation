using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("Tenants");

        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Code).HasMaxLength(100);
        builder.Property(t => t.Email).HasMaxLength(100);
        builder.Property(t => t.PhoneNumber).HasMaxLength(25);
    }
}
