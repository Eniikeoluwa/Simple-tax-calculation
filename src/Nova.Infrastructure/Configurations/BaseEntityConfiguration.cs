using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nova.Infrastructure.Configurations;

public class BaseConfiguration : IEntityTypeConfiguration<BaseEntity>
{
    public void Configure(EntityTypeBuilder<BaseEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.UpdatedBy).IsRequired();
        builder.Property(e => e.IsDeleted).IsRequired();
    }
}
