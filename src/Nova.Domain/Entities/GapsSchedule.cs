using System;

namespace Nova.Domain.Entities;

public class GapsSchedule : BaseEntity
{
    public string BatchNumber { get; set; } = null!;
    public decimal PaymentAmount { get; set; } 
    public DateTime PaymentDate { get; set; } 
    public string Reference { get; set; } = null!;  
    public string Remark { get; set; } = null!;   
    public string VendorCode { get; set; } = null!; 
    public string VendorName { get; set; } = null!; 
    public string VendorAccountNumber { get; set; } = null!; 
    public string VendorBankSortCode { get; set; } = null!;
    public string VendorBankName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? UploadedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string ProcessingNotes { get; set; } = null!;
    
    public string BulkScheduleId { get; set; } = null!;
    public string PaymentId { get; set; } = null!;
    public string VendorId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string? ProcessedByUserId { get; set; }

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
