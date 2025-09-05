using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("RefreshTokens");

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.RevokedAt);

        // Relationships
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);
    }
}
