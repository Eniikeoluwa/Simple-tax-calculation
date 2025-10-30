namespace Nova.Domain.Entities;

public class Payment : BaseEntity
{
    public string InvoiceNumber { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TaxableAmount { get; set; }
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
    public virtual Payment ParentPayment { get; set; }
    public virtual ICollection<Payment> ChildPayments { get; set; }

    public bool HasVatApplied => AppliedTaxType == "VAT" || AppliedTaxType == "Both";
    public bool HasWhtApplied => AppliedTaxType == "WHT" || AppliedTaxType == "Both";
    public bool HasNoTaxApplied => AppliedTaxType == "None";

    public void CalculateNetAmount()
    {
        if (IsPartialPayment && !IsFinalPayment)
        {
            VatAmount = 0;
            WhtAmount = 0;
            NetAmount = PaymentAmount;
        }
        else if (IsFinalPayment)
        {
            var originalVatAmount = HasVatApplied ? TaxableAmount * (AppliedVatRate / 100) : 0;
            var originalWhtAmount = HasWhtApplied ? TaxableAmount * (AppliedWhtRate / 100) : 0;

            VatAmount = originalVatAmount;
            WhtAmount = originalWhtAmount;
            NetAmount = GrossAmount - VatAmount - WhtAmount;
        }
        else
        {
            VatAmount = HasVatApplied ? TaxableAmount * (AppliedVatRate / 100) : 0;
            WhtAmount = HasWhtApplied ? TaxableAmount * (AppliedWhtRate / 100) : 0;
            NetAmount = GrossAmount - VatAmount - WhtAmount;
        }
    }

    public Payment()
    {
        InvoiceNumber = string.Empty;
        Description = string.Empty;
        Reference = string.Empty;
        Remarks = string.Empty;
        VendorId = string.Empty;
        TenantId = string.Empty;
        CreatedByUserId = string.Empty;
        Status = "Pending";
        AppliedTaxType = "Both";
        IsPartialPayment = false;
        IsFinalPayment = false;
        TotalAmountPaid = 0;
        ChildPayments = new HashSet<Payment>();
        Tenant = null!;
        Vendor = null!;
        BulkSchedule = null!;
        CreatedByUser = null!;
        ParentPayment = null!;
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
