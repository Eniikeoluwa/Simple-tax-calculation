namespace Nova.Domain.Entities;

public class TenantUser : BaseEntity
{
    public string UserId { get; private set; } = null!;
    public string TenantId { get; private set; } = null!;
    public string RoleString { get; private set; } = null!;
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User User { get; set; } = null!;

    public TenantUser()
    {
        UserId = Guid.NewGuid().ToString();
        TenantId = Guid.NewGuid().ToString();
        RoleString = string.Empty;
    }
}