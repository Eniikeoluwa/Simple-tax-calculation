using System;

namespace Nova.Domain.Entities;

public class GapsSchedule : BaseEntity
{
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Reference { get; set; } = null!;
    public string Remark { get; set; } = null!;
    public string VendorCode { get; set; } = null!;
    public string VendorName { get; set; } = null!;
    public string VendorAccountNumber { get; set; } = null!;
    public string VendorBankSortCode { get; set; } = null!;
    public string VendorBankName { get; set; } = null!;
    public string Status { get; set; } = null!; // Generated, Uploaded, Processed, Failed
    public DateTime? UploadedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string ProcessingNotes { get; set; } = null!;
    public string BulkScheduleId { get; set; } = null!;
    public string PaymentId { get; set; } = null!;
    public string VendorId { get; set; } = null!;

    public virtual BulkSchedule BulkSchedule { get; set; }
    public virtual Payment Payment { get; set; }
    public virtual Vendor Vendor { get; set; }

    public GapsSchedule()
    {
        Status = "Generated";
    }
}
