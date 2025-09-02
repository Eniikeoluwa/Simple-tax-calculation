namespace Nova.Domain.Entities;

public class Vendor : BaseEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string AccountName { get; set; }
    public string AccountNumber { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string TaxIdentificationNumber { get; set; }
    public string TaxType { get; set; } // "None", "VAT", "WHT", "Both"
    public decimal VatRate { get; set; } = 7.5m;
    public decimal WhtRate { get; set; } = 2.0m;
    public bool IsActive { get; set; }
    public string BankId { get; set; }

    public virtual Bank Bank { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }

    public bool HasVat => TaxType == "VAT" || TaxType == "Both";
    public bool HasWht => TaxType == "WHT" || TaxType == "Both";
    public bool HasNoTax => TaxType == "None";

    public Vendor()
    {
        Payments = new HashSet<Payment>();
        IsActive = true;
        TaxType = "Both";
    }
}
