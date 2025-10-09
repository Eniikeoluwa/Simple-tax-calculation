using System;

namespace Nova.Domain.Entities;

public class GapsSchedule : BaseEntity
{
    public string BatchNumber { get; set; } = null!;
    public decimal PaymentAmount { get; set; }  // 2 decimal places max
    public DateTime PaymentDate { get; set; }   // dd/mmm/yyyy format
    public string Reference { get; set; } = null!;  // alpha-numeric, max 20 chars
    public string Remark { get; set; } = null!;     // alpha-numeric, max 25 chars
    public string VendorCode { get; set; } = null!; // alpha-numeric, max 32 chars
    public string VendorName { get; set; } = null!; // max 50 chars
    public string VendorAccountNumber { get; set; } = null!; // numeric, max 15 digits
    public string VendorBankSortCode { get; set; } = null!; // 9 digits
    public string VendorBankName { get; set; } = null!;
    public string Status { get; set; } = null!; // Generated, Uploaded, Processed, Failed
    public DateTime? UploadedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string ProcessingNotes { get; set; } = null!;
    
    // Foreign keys
    public string BulkScheduleId { get; set; } = null!;
    public string PaymentId { get; set; } = null!;
    public string VendorId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string? ProcessedByUserId { get; set; }

    // Navigation properties
    public virtual BulkSchedule BulkSchedule { get; set; } = null!;
    public virtual Payment Payment { get; set; } = null!;
    public virtual Vendor Vendor { get; set; } = null!;
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? ProcessedByUser { get; set; }

    public GapsSchedule()
    {
        Status = "Generated";
        ProcessingNotes = string.Empty;
    }
}
