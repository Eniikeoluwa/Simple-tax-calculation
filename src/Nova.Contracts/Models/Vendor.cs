namespace Nova.Contracts.Vendor;

public class CreateVendorRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxIdentificationNumber { get; set; } = string.Empty;
    public string TaxType { get; set; } = "Both";
    public decimal VatRate { get; set; } = 7.5m;
    public decimal WhtRate { get; set; } = 2.0m;
    public string BankId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

public class VendorResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxIdentificationNumber { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal WhtRate { get; set; }
    public bool IsActive { get; set; }
    public string BankId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetVendorsResponse
{
    public List<VendorResponse> Vendors { get; set; } = new();
}

public class GetVendorsRequest
{
    public string TenantId { get; set; } = string.Empty;
}
