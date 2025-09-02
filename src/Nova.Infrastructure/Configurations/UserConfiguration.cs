using Nova.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nova.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(50);

        builder.Property(e => e.Email).HasMaxLength(100).IsRequired();

        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();

        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();

        builder.Property(e => e.Password).HasMaxLength(100).IsRequired();

        builder.Property(e => e.PhoneNumber).HasMaxLength(25).IsRequired();

        builder.Property(e => e.CurrentTenantId).HasMaxLength(50);

        builder.HasMany(e => e.TenantUsers).WithOne(tu => tu.User).HasForeignKey(tu => tu.UserId);
    }
}
