using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("TenantUsers");

        builder.Property(tu => tu.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(tu => tu.UserId).HasMaxLength(50).IsRequired();
        builder.Property(tu => tu.RoleString).HasMaxLength(200);

        builder.HasOne(e => e.Tenant).WithMany(t => t.TenantUsers).HasForeignKey(e => e.TenantId);
        builder.HasOne(tu => tu.User).WithMany(u => u.TenantUsers).HasForeignKey(tu => tu.UserId);
    }
}
