namespace Nova.Domain.Entities;

public class Bank : BaseEntity
{
    public string Name { get; set; } = null!;
    public string SortCode { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsActive { get; set; }

    public virtual ICollection<Vendor> Vendors { get; set; }

    public Bank()
    {
        Vendors = new HashSet<Vendor>();
        IsActive = true;
    }
}