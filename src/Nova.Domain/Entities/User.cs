namespace Nova.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string? TenantId { get; set; }
    public virtual ICollection<Payment> CreatedPayments { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    public virtual ICollection<TenantUser> TenantUsers { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public User()
    {
    CreatedPayments = new HashSet<Payment>();
        RefreshTokens = new HashSet<RefreshToken>();
        TenantUsers = new HashSet<TenantUser>();
        IsActive = true;
    }
}