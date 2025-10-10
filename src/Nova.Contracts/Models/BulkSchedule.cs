namespace Nova.Contracts.Models;

public class CreateBulkScheduleRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

public class BulkScheduleResponse
{
    public string Id { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal TotalWhtAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int PaymentCount { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PaymentResponse> Payments { get; set; } = new();
}

public class BulkScheduleListResponse
{
    public string Id { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalVatAmount { get; set; }
    public decimal TotalWhtAmount { get; set; }
    public decimal TotalNetAmount { get; set; }
    public int PaymentCount { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateBulkScheduleStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

public class ApproveBulkScheduleRequest
{
    public string Remarks { get; set; } = string.Empty;
}

public class BulkScheduleExportResponse
{
    public byte[] FileContent { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}