namespace Nova.Contracts.Models;

public class CreatePaymentRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
}

public class PaymentResponse
{
    public string Id { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal WhtAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string AppliedTaxType { get; set; } = string.Empty;
    public decimal AppliedVatRate { get; set; }
    public decimal AppliedWhtRate { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string BulkScheduleId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string ApprovedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related entities
    public VendorInfo? Vendor { get; set; }
    public UserInfo? CreatedByUser { get; set; }
    public UserInfo? ApprovedByUser { get; set; }
}

public class VendorInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal WhtRate { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdatePaymentStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
}
