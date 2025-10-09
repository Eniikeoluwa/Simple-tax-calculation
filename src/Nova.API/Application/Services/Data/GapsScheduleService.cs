using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;
using Nova.Domain.Utils;
using Nova.Infrastructure;
using Nova.Contracts.Models;
using System.Text;
using FluentResults;
using Nova.API.Application.Services.Common;
using Nova.API.Application.Helpers;
using ClosedXML.Excel;
using Nova.Infrastructure;
using Nova.Contracts.Models;


namespace Nova.API.Application.Services.Data;

public interface IGapsScheduleService
{
    Task<Result<List<GapsScheduleResponse>>> GenerateGapsScheduleAsync(GenerateGapsScheduleRequest request);
    Task<Result<List<GapsScheduleListResponse>>> GetGapsSchedulesForCurrentTenantAsync();
    Task<Result<GapsScheduleResponse>> GetGapsScheduleByIdAsync(string id);
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

    public async Task<Result<List<GapsScheduleResponse>>> GenerateGapsScheduleAsync(GenerateGapsScheduleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            // Get the bulk schedule with its payments
            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.Payments)
                .ThenInclude(p => p.Vendor)
                .ThenInclude(v => v.Bank)
                .FirstOrDefaultAsync(bs => bs.Id == request.BulkScheduleId && bs.TenantId == TenantId);

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            if (bulkSchedule.Status != "Approved")
                return Result.Fail("Only approved bulk schedules can be used to generate GAPS schedule");

            var gapsSchedules = new List<GapsSchedule>();
            var batchNumber = $"GAPS-{DateTime.Now:yyyyMMdd-HHmmss}";

            foreach (var payment in bulkSchedule.Payments)
            {
                var vendor = payment.Vendor;

                // Skip if vendor doesn't have bank sort code
                if (string.IsNullOrEmpty(vendor.Bank?.SortCode))
                    continue;

                var gapsSchedule = new GapsSchedule
                {
                    BatchNumber = batchNumber,
                    PaymentAmount = payment.NetAmount,
                    PaymentDate = request.PaymentDate,
                    Reference = payment.Reference ?? payment.InvoiceNumber,
                    Remark = payment.Description.Length > 25 ? payment.Description[..25] : payment.Description,
                    VendorCode = vendor.Code,
                    VendorName = vendor.Name,
                    VendorAccountNumber = vendor.AccountNumber,
                    VendorBankSortCode = vendor.Bank.SortCode,
                    VendorBankName = vendor.Bank.Name,
                    BulkScheduleId = bulkSchedule.Id,
                    PaymentId = payment.Id,
                    VendorId = vendor.Id,
                    TenantId = TenantId,
                    CreatedByUserId = payment.CreatedByUserId,
                    Status = "Generated"
                };

                gapsSchedules.Add(gapsSchedule);
            }

            _context.GapsSchedules.AddRange(gapsSchedules);
            await _context.SaveChangesAsync();

            var response = gapsSchedules.Select(gs => new GapsScheduleResponse
            {
                Id = gs.Id,
                BatchNumber = gs.BatchNumber,
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
                BulkScheduleId = gs.BulkScheduleId,
                CreatedByUserId = gs.CreatedByUserId,
                CreatedAt = gs.CreatedAt
            }).ToList();

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
                .OrderByDescending(gs => gs.CreatedAt)
                .Select(gs => new GapsScheduleListResponse
                {
                    Id = gs.Id,
                    BatchNumber = gs.BatchNumber,
                    PaymentAmount = gs.PaymentAmount,
                    PaymentDate = gs.PaymentDate,
                    VendorName = gs.VendorName,
                    Status = gs.Status,
                    CreatedAt = gs.CreatedAt
                })
                .ToListAsync();

            return Result.Ok(gapsSchedules);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving GAPS schedules: {ex.Message}");
        }
    }

    public async Task<Result<GapsScheduleResponse>> GetGapsScheduleByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var gapsSchedule = await _context.GapsSchedules
                .FirstOrDefaultAsync(gs => gs.Id == id && gs.TenantId == TenantId);

            if (gapsSchedule == null)
                return Result.Fail("GAPS schedule not found");

            var response = new GapsScheduleResponse
            {
                Id = gapsSchedule.Id,
                BatchNumber = gapsSchedule.BatchNumber,
                PaymentAmount = gapsSchedule.PaymentAmount,
                PaymentDate = gapsSchedule.PaymentDate,
                Reference = gapsSchedule.Reference,
                Remark = gapsSchedule.Remark,
                VendorCode = gapsSchedule.VendorCode,
                VendorName = gapsSchedule.VendorName,
                VendorAccountNumber = gapsSchedule.VendorAccountNumber,
                VendorBankSortCode = gapsSchedule.VendorBankSortCode,
                VendorBankName = gapsSchedule.VendorBankName,
                Status = gapsSchedule.Status,
                UploadedDate = gapsSchedule.UploadedDate,
                ProcessedDate = gapsSchedule.ProcessedDate,
                ProcessingNotes = gapsSchedule.ProcessingNotes,
                BulkScheduleId = gapsSchedule.BulkScheduleId,
                CreatedByUserId = gapsSchedule.CreatedByUserId,
                CreatedAt = gapsSchedule.CreatedAt
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

            // Add headers
            worksheet.Cell(1, 1).Value = "Payment Amount";
            worksheet.Cell(1, 2).Value = "Payment Date";
            worksheet.Cell(1, 3).Value = "Reference";
            worksheet.Cell(1, 4).Value = "Remark";
            worksheet.Cell(1, 5).Value = "Vendor Code";
            worksheet.Cell(1, 6).Value = "Vendor Name";
            worksheet.Cell(1, 7).Value = "Account Number";
            worksheet.Cell(1, 8).Value = "Bank Sort Code";

            // Add data
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

            // Format columns
            worksheet.Columns().AdjustToContents();

            // Convert to byte array
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
