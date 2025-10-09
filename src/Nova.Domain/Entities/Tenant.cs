namespace Nova.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<TenantUser> TenantUsers { get; set; }
    public virtual ICollection<Vendor> Vendors { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<BulkSchedule> BulkSchedules { get; set; }

    public Tenant()
    {
        Users = new HashSet<User>();
        TenantUsers = new HashSet<TenantUser>();
        Vendors = new HashSet<Vendor>();
        Payments = new HashSet<Payment>();
        BulkSchedules = new HashSet<BulkSchedule>();
        IsActive = true;
    }
}
