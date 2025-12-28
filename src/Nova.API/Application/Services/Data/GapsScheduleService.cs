using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;
using FluentResults;
using Nova.API.Application.Services.Common;
using ClosedXML.Excel;


namespace Nova.API.Application.Services.Data;

public interface IGapsScheduleService
{
    Task<Result<GapsScheduleResponse>> GenerateGapsScheduleAsync(GenerateGapsScheduleRequest request);
    Task<Result<List<GapsScheduleListResponse>>> GetGapsSchedulesForCurrentTenantAsync();
    Task<Result<GapsScheduleResponse>> GetGapsScheduleByIdAsync(string batchNumber);
    Task<Result<GapsScheduleExportResponse>> ExportToExcelAsync(string batchNumber);
}

public class GapsScheduleService : BaseDataService, IGapsScheduleService
{
    private readonly ICurrentUserService _currentUserService;

    public GapsScheduleService(AppDbContext context, ICurrentUserService currentUserService) : base(context)
    {
        _currentUserService = currentUserService;
    }
    private string TenantId => _currentUserService.TenantId;

    public async Task<Result<GapsScheduleResponse>> GenerateGapsScheduleAsync(GenerateGapsScheduleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.Payments)
                .ThenInclude(p => p.Vendor)
                .ThenInclude(v => v.Bank)
                .FirstOrDefaultAsync(bs => bs.Id == request.BulkScheduleId && bs.TenantId == TenantId);

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            if (bulkSchedule.Status != "Approved")
                return Result.Fail($"Only approved bulk schedules can be used to generate GAPS schedule. Current status: {bulkSchedule.Status}. Please approve the bulk schedule first.");

            if (!bulkSchedule.Payments.Any())
                return Result.Fail("No payments are linked to this bulk schedule. Please ensure payments are properly associated with the bulk schedule.");


            var batchNumber = $"GAPS-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            var gapsSchedules = new List<GapsSchedule>();

            foreach (var payment in bulkSchedule.Payments)
            {
                var vendor = payment.Vendor;
                if (vendor == null) continue;

                var gapsSchedule = new GapsSchedule
                {
                    BatchNumber = batchNumber,
                    PaymentAmount = payment.NetAmount,
                    PaymentDate = request.PaymentDate,
                    Reference = !string.IsNullOrWhiteSpace(payment.Reference) ? payment.Reference : payment.InvoiceNumber ?? "",
                    Remark = !string.IsNullOrWhiteSpace(payment.Description) && payment.Description.Length > 25 
                        ? payment.Description[..25] 
                        : payment.Description ?? "",
                    VendorCode = vendor.Code ?? "",
                    VendorName = vendor.Name ?? "",
                    VendorAccountNumber = vendor.AccountNumber ?? "",
                    VendorBankSortCode = vendor.Bank?.SortCode ?? "",
                    VendorBankName = vendor.Bank?.Name ?? "",
                    BulkScheduleId = bulkSchedule.Id,
                    PaymentId = payment.Id,
                    VendorId = vendor.Id,
                    TenantId = TenantId,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedBy = _currentUserService.UserId,
                    UpdatedBy = _currentUserService.UserId,
                    Status = "Generated"
                };

                gapsSchedules.Add(gapsSchedule);
            }

            if (!gapsSchedules.Any())
                return Result.Fail("No valid payments found to generate GAPS schedules.");

            _context.GapsSchedules.AddRange(gapsSchedules);
            await _context.SaveChangesAsync();

            var response = new GapsScheduleResponse
            {
                Id = batchNumber, 
                BatchNumber = batchNumber,
                TotalPaymentAmount = gapsSchedules.Sum(gs => gs.PaymentAmount),
                PaymentDate = request.PaymentDate,
                TotalItems = gapsSchedules.Count,
                Status = "Generated",
                BulkScheduleId = bulkSchedule.Id,
                CreatedByUserId = _currentUserService.UserId,
                CreatedAt = DateTime.UtcNow,
                Items = gapsSchedules.Select(gs => new GapsScheduleItemResponse
                {
                    Id = gs.Id,
                    PaymentAmount = gs.PaymentAmount,
                    PaymentDate = gs.PaymentDate,
                    Reference = gs.Reference,
                    Remark = gs.Remark,
                    VendorCode = gs.VendorCode,
                    VendorName = gs.VendorName,
                    VendorAccountNumber = gs.VendorAccountNumber,
                    VendorBankSortCode = gs.VendorBankSortCode,
                    VendorBankName = gs.VendorBankName,
                    Status = gs.Status,
                    UploadedDate = gs.UploadedDate,
                    ProcessedDate = gs.ProcessedDate,
                    ProcessingNotes = gs.ProcessingNotes,
                    PaymentId = gs.PaymentId,
                    VendorId = gs.VendorId,
                    CreatedAt = gs.CreatedAt
                }).ToList()
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while generating GAPS schedule: {ex.Message}");
        }
    }

    public async Task<Result<List<GapsScheduleListResponse>>> GetGapsSchedulesForCurrentTenantAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var gapsSchedules = await _context.GapsSchedules
                .Where(gs => gs.TenantId == TenantId)
                .GroupBy(gs => gs.BatchNumber)
                .Select(g => new GapsScheduleListResponse
                {
                    Id = g.Key, 
                    BatchNumber = g.Key,
                    TotalPaymentAmount = g.Sum(gs => gs.PaymentAmount),
                    PaymentDate = g.First().PaymentDate,
                    TotalItems = g.Count(),
                    Status = g.First().Status,
                    CreatedAt = g.Min(gs => gs.CreatedAt)
                })
                .OrderByDescending(gs => gs.CreatedAt)
                .ToListAsync();

            return Result.Ok(gapsSchedules);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving GAPS schedules: {ex.Message}");
        }
    }

    public async Task<Result<GapsScheduleResponse>> GetGapsScheduleByIdAsync(string batchNumber)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var gapsSchedules = await _context.GapsSchedules
                .Where(gs => gs.BatchNumber == batchNumber && gs.TenantId == TenantId)
                .OrderBy(gs => gs.CreatedAt)
                .ToListAsync();

            if (!gapsSchedules.Any())
                return Result.Fail("GAPS schedule not found");

            var firstGapsSchedule = gapsSchedules.First();
            var response = new GapsScheduleResponse
            {
                Id = batchNumber,
                BatchNumber = batchNumber,
                TotalPaymentAmount = gapsSchedules.Sum(gs => gs.PaymentAmount),
                PaymentDate = firstGapsSchedule.PaymentDate,
                TotalItems = gapsSchedules.Count,
                Status = firstGapsSchedule.Status,
                BulkScheduleId = firstGapsSchedule.BulkScheduleId,
                CreatedByUserId = firstGapsSchedule.CreatedByUserId,
                CreatedAt = firstGapsSchedule.CreatedAt,
                Items = gapsSchedules.Select(gs => new GapsScheduleItemResponse
                {
                    Id = gs.Id,
                    PaymentAmount = gs.PaymentAmount,
                    PaymentDate = gs.PaymentDate,
                    Reference = gs.Reference,
                    Remark = gs.Remark,
                    VendorCode = gs.VendorCode,
                    VendorName = gs.VendorName,
                    VendorAccountNumber = gs.VendorAccountNumber,
                    VendorBankSortCode = gs.VendorBankSortCode,
                    VendorBankName = gs.VendorBankName,
                    Status = gs.Status,
                    UploadedDate = gs.UploadedDate,
                    ProcessedDate = gs.ProcessedDate,
                    ProcessingNotes = gs.ProcessingNotes,
                    PaymentId = gs.PaymentId,
                    VendorId = gs.VendorId,
                    CreatedAt = gs.CreatedAt
                }).ToList()
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving GAPS schedule: {ex.Message}");
        }
    }

    public async Task<Result<GapsScheduleExportResponse>> ExportToExcelAsync(string batchNumber)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var gapsSchedules = await _context.GapsSchedules
                .Where(gs => gs.BatchNumber == batchNumber && gs.TenantId == TenantId)
                .OrderBy(gs => gs.CreatedAt)
                .ToListAsync();

            if (!gapsSchedules.Any())
                return Result.Fail("No GAPS schedules found for the specified batch");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("GAPS Schedule");

            worksheet.Cell(1, 1).Value = "Payment Amount";
            worksheet.Cell(1, 2).Value = "Payment Date";
            worksheet.Cell(1, 3).Value = "Reference";
            worksheet.Cell(1, 4).Value = "Remark";
            worksheet.Cell(1, 5).Value = "Vendor Code";
            worksheet.Cell(1, 6).Value = "Vendor Name";
            worksheet.Cell(1, 7).Value = "Account Number";
            worksheet.Cell(1, 8).Value = "Bank Sort Code";

            var row = 2;
            foreach (var gaps in gapsSchedules)
            {
                worksheet.Cell(row, 1).Value = gaps.PaymentAmount;
                worksheet.Cell(row, 2).Value = gaps.PaymentDate.ToString("dd/MMM/yyyy");
                worksheet.Cell(row, 3).Value = gaps.Reference;
                worksheet.Cell(row, 4).Value = gaps.Remark;
                worksheet.Cell(row, 5).Value = gaps.VendorCode;
                worksheet.Cell(row, 6).Value = gaps.VendorName;
                worksheet.Cell(row, 7).Value = gaps.VendorAccountNumber;
                worksheet.Cell(row, 8).Value = gaps.VendorBankSortCode;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var response = new GapsScheduleExportResponse
            {
                FileContent = content,
                FileName = $"GAPS_Schedule_{batchNumber}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while exporting GAPS schedule: {ex.Message}");
        }
    }
}
