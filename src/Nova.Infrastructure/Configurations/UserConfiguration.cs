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

        builder.Property(e => e.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(25);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.LastLoginAt);

        // Relationships
        builder.HasMany(e => e.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.Email)
            .IsUnique();
    }
}
