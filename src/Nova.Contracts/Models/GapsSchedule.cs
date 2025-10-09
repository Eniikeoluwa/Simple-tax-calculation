using System;

namespace Nova.Contracts.Models;

public class GapsScheduleResponse
{
    public string Id { get; set; } = null!;
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
    public string CreatedByUserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class GapsScheduleListResponse
{
    public string Id { get; set; } = null!;
    public string BatchNumber { get; set; } = null!;
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string VendorName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class GenerateGapsScheduleRequest
{
    public string BulkScheduleId { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
}

public class GapsScheduleExportResponse
{
    public byte[] FileContent { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
