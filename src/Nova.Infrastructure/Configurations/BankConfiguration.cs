using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nova.Domain.Entities;

namespace Nova.Infrastructure.Configurations;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ConfigureBase();
        builder.ToTable("Banks");

        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.SortCode).HasMaxLength(50);
        builder.Property(b => b.Code).HasMaxLength(50);
    }
}
