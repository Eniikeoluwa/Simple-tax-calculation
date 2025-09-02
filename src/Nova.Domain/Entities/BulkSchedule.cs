namespace Nova.Domain.Entities;

public class BulkSchedule : BaseEntity
{
    public string BatchNumber { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal TotalWhtAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int PaymentCount { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string Status { get; set; } // Draft, Ready, Processed, Completed, Cancelled
    public string Remarks { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string ProcessedByUserId { get; set; } = null!;

    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User ProcessedByUser { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = null!;
    public virtual ICollection<GapsSchedule> GapsSchedules { get; set; } = null!;

    public BulkSchedule()
    {
        Payments = new HashSet<Payment>();
        GapsSchedules = new HashSet<GapsSchedule>();
        Status = "Draft";
    }
}
