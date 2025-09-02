namespace Nova.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public string Password { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? CurrentTenantId { get; set; }

    public virtual ICollection<TenantUser> TenantUsers { get; set; }
    public virtual ICollection<Payment> CreatedPayments { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public User()
    {
        CreatedPayments = new HashSet<Payment>();
        IsActive = true;
    }
}