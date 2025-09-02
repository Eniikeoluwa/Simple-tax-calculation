
using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;

namespace Nova.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<BulkSchedule> BulkSchedules { get; set; }
    public DbSet<GapsSchedule> GapsSchedules { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Vendor> Vendors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.BankConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TenantUserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.BulkScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.GapsScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TenantConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.VendorConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
