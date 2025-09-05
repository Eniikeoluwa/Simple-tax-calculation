namespace Nova.Domain.Entities;

public class TenantUser : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string Role { get; set; } = null!;
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User User { get; set; } = null!;

    public TenantUser()
    {
        UserId = string.Empty;
        TenantId = string.Empty;
        Role = string.Empty;
    }
}