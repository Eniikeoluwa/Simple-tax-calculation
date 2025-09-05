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

        builder.Property(tu => tu.TenantId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(tu => tu.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(tu => tu.Role)
            .HasMaxLength(50)
            .IsRequired();

        // Relationships
        builder.HasOne(tu => tu.Tenant)
            .WithMany(t => t.TenantUsers)
            .HasForeignKey(tu => tu.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tu => tu.User)
            .WithMany(u => u.TenantUsers)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(tu => new { tu.UserId, tu.TenantId })
            .IsUnique();

        builder.HasIndex(tu => tu.TenantId);
        builder.HasIndex(tu => tu.UserId);
    }
}
