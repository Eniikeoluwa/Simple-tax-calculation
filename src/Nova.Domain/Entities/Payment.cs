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
    public string Status { get; set; } 
    public string Remarks { get; set; }
    public string VendorId { get; set; }
    public string? BulkScheduleId { get; set; }
    public string TenantId { get; set; }
    public string CreatedByUserId { get; set; }
    public string? ApprovedByUserId { get; set; }
    public decimal OriginalInvoiceAmount { get; set; } 
    public decimal PaymentAmount { get; set; } 
    public bool IsPartialPayment { get; set; } 
    public bool IsFinalPayment { get; set; } 
    public string? ParentPaymentId { get; set; } 
    public decimal TotalAmountPaid { get; set; }

    public virtual Tenant Tenant { get; set; }
    public virtual Vendor Vendor { get; set; }
    public virtual BulkSchedule BulkSchedule { get; set; }
    public virtual User CreatedByUser { get; set; }
    public virtual User ApprovedByUser { get; set; }
    
    public virtual Payment ParentPayment { get; set; }
    public virtual ICollection<Payment> ChildPayments { get; set; }

    public bool HasVatApplied => AppliedTaxType == "VAT" || AppliedTaxType == "Both";
    public bool HasWhtApplied => AppliedTaxType == "WHT" || AppliedTaxType == "Both";
    public bool HasNoTaxApplied => AppliedTaxType == "None";

    public void CalculateNetAmount()
    {
        if (IsPartialPayment && !IsFinalPayment)
        {
            // For first partial payments, no tax deductions applied yet
            VatAmount = 0;
            WhtAmount = 0;
            NetAmount = PaymentAmount; // Net equals payment amount (no deductions)
        }
        else if (IsFinalPayment)
        {
            // For final payments, calculate tax from original invoice amount and deduct from this payment
            var originalVatAmount = HasVatApplied ? OriginalInvoiceAmount * (AppliedVatRate / 100) : 0;
            var originalWhtAmount = HasWhtApplied ? OriginalInvoiceAmount * (AppliedWhtRate / 100) : 0;
            
            VatAmount = originalVatAmount;
            WhtAmount = originalWhtAmount;
            // Deduct all taxes from the final payment amount
            NetAmount = PaymentAmount - VatAmount - WhtAmount;
        }
        else
        {
            // For full payments (non-partial), calculate taxes and deduct from gross amount
            VatAmount = HasVatApplied ? GrossAmount * (AppliedVatRate / 100) : 0;
            WhtAmount = HasWhtApplied ? GrossAmount * (AppliedWhtRate / 100) : 0;
            // NetAmount is what vendor receives after tax deductions
            NetAmount = GrossAmount - VatAmount - WhtAmount;
        }
    }

    public Payment()
    {
        Status = "Pending";
        AppliedTaxType = "Both";
        IsPartialPayment = false;
        IsFinalPayment = false;
        TotalAmountPaid = 0;
        ChildPayments = new HashSet<Payment>();
    }
    
    // Helper methods for two-payment system
    public decimal GetRemainingBalance()
    {
        return OriginalInvoiceAmount - TotalAmountPaid;
    }
    
    public decimal GetRemainingPercentage()
    {
        if (OriginalInvoiceAmount == 0) return 0;
        return GetRemainingBalance() / OriginalInvoiceAmount * 100;
    }
    
    public void SetAsFirstPartialPayment(decimal grossAmount, decimal percentage)
    {
        OriginalInvoiceAmount = grossAmount;
        PaymentAmount = grossAmount * (percentage / 100);
        TotalAmountPaid = PaymentAmount;
        IsPartialPayment = true;
        IsFinalPayment = false;
        GrossAmount = PaymentAmount; 
    }
    
    public void SetAsFinalPayment(decimal grossAmount, decimal firstPaymentAmount)
    {
        OriginalInvoiceAmount = grossAmount;
        PaymentAmount = grossAmount - firstPaymentAmount; 
        TotalAmountPaid = grossAmount;
        IsPartialPayment = true;
        IsFinalPayment = true;
        GrossAmount = PaymentAmount;
    }
}
