namespace Nova.Domain.Entities;

public class Payment : BaseEntity
{
    public string InvoiceNumber { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal WhtAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Description { get; set; }
    public string Reference { get; set; }
    public string AppliedTaxType { get; set; } // "None", "VAT", "WHT", "Both"
    public decimal AppliedVatRate { get; set; }
    public decimal AppliedWhtRate { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Status { get; set; } // Pending, Approved, Processed, Paid, Cancelled
    public string Remarks { get; set; }
    public string VendorId { get; set; }
    public string BulkScheduleId { get; set; }
    public string CreatedByUserId { get; set; }
    public string ApprovedByUserId { get; set; }

    public virtual Vendor Vendor { get; set; }
    public virtual BulkSchedule BulkSchedule { get; set; }
    public virtual User CreatedByUser { get; set; }
    public virtual User ApprovedByUser { get; set; }

    public bool HasVatApplied => AppliedTaxType == "VAT" || AppliedTaxType == "Both";
    public bool HasWhtApplied => AppliedTaxType == "WHT" || AppliedTaxType == "Both";
    public bool HasNoTaxApplied => AppliedTaxType == "None";

    public void CalculateNetAmount()
    {
        VatAmount = HasVatApplied ? GrossAmount * (AppliedVatRate / 100) : 0;
        WhtAmount = HasWhtApplied ? GrossAmount * (AppliedWhtRate / 100) : 0;
        NetAmount = GrossAmount - VatAmount - WhtAmount;
    }

    public Payment()
    {
        Status = "Pending";
        AppliedTaxType = "Both";
    }
}
