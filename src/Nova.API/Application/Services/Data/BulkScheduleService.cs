using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.API.Application.Services.Common;
using Nova.API.Application.Helpers;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;
using System.Text;

namespace Nova.API.Application.Services.Data;

public interface IBulkScheduleService
{
    Task<Result<BulkScheduleResponse>> GenerateBulkScheduleAsync(CreateBulkScheduleRequest request);
    Task<Result<List<BulkScheduleListResponse>>> GetBulkSchedulesForCurrentTenantAsync();
    Task<Result<BulkScheduleResponse>> GetBulkScheduleByIdAsync(string bulkScheduleId);
    Task<Result<bool>> UpdateBulkScheduleStatusAsync(string bulkScheduleId, UpdateBulkScheduleStatusRequest request);
    Task<Result<bool>> ApproveBulkScheduleAsync(string bulkScheduleId, ApproveBulkScheduleRequest request);
    Task<Result<bool>> DeleteBulkScheduleAsync(string bulkScheduleId);
    Task<Result<BulkScheduleExportResponse>> ExportBulkScheduleToCsvAsync(string bulkScheduleId);
}

public class BulkScheduleService : BaseDataService, IBulkScheduleService
{
    private readonly ICurrentUserService _currentUserService;

    public BulkScheduleService(AppDbContext context, ICurrentUserService currentUserService) : base(context)
    {
        _currentUserService = currentUserService;
    }

    private string TenantId => _currentUserService.TenantId;
    private string UserId => _currentUserService.UserId;

    public async Task<Result<BulkScheduleResponse>> GenerateBulkScheduleAsync(CreateBulkScheduleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            if (string.IsNullOrEmpty(UserId))
                return Result.Fail("User not authenticated");

            if (request.StartDate > request.EndDate)
                return Result.Fail("Start date cannot be later than end date");

            var startDateUtc = DateTimeHelper.EnsureUtc(request.StartDate);
            var endDateUtc = DateTimeHelper.EnsureUtc(request.EndDate).AddDays(1).AddTicks(-1); 
            
            var payments = await _context.Payments
                .Include(p => p.Vendor)
                .ThenInclude(v => v.Bank)
                .Include(p => p.CreatedByUser)
                .Where(p => p.TenantId == TenantId
                         && p.CreatedAt >= startDateUtc
                         && p.CreatedAt <= endDateUtc)
                .OrderBy(p => p.Vendor.Name)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync();

            if (!payments.Any())
                return Result.Fail($"No approved payments found created between {request.StartDate:yyyy-MM-dd} and {request.EndDate:yyyy-MM-dd}");

            var totalGrossAmount = payments.Sum(p => p.GrossAmount);
            var totalVatAmount = payments.Sum(p => p.VatAmount);
            var totalWhtAmount = payments.Sum(p => p.WhtAmount);
            var totalNetAmount = payments.Sum(p => p.NetAmount);

            var batchNumber = await GenerateBatchNumberAsync();

            var bulkSchedule = new BulkSchedule
            {
                BatchNumber = batchNumber,
                Description = request.Description ?? $"Bulk Schedule - Payments from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}",
                TotalGrossAmount = totalGrossAmount,
                TotalVatAmount = totalVatAmount,
                TotalWhtAmount = totalWhtAmount,
                TotalNetAmount = totalNetAmount,
                PaymentCount = payments.Count,
                ScheduledDate = DateTimeHelper.EnsureUtc(DateTime.Now),
                Status = "Pending",
                Remarks = request.Remarks ?? "",
                TenantId = TenantId,
                CreatedByUserId = UserId,
                CreatedBy = UserId,
                UpdatedBy = UserId,
                StartDate = startDateUtc,  
                EndDate = endDateUtc
            };

            _context.BulkSchedules.Add(bulkSchedule);
            await _context.SaveChangesAsync();

            foreach (var payment in payments)
            {
                payment.BulkScheduleId = bulkSchedule.Id;
                payment.UpdatedBy = UserId;
                payment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var response = new BulkScheduleResponse
            {
                Id = bulkSchedule.Id,
                BatchNumber = bulkSchedule.BatchNumber,
                Description = bulkSchedule.Description,
                TotalGrossAmount = bulkSchedule.TotalGrossAmount,
                TotalVatAmount = bulkSchedule.TotalVatAmount,
                TotalWhtAmount = bulkSchedule.TotalWhtAmount,
                TotalNetAmount = bulkSchedule.TotalNetAmount,
                PaymentCount = bulkSchedule.PaymentCount,
                ScheduledDate = bulkSchedule.ScheduledDate,
                ProcessedDate = bulkSchedule.ProcessedDate,
                Status = bulkSchedule.Status,
                Remarks = bulkSchedule.Remarks,
                CreatedByUserId = bulkSchedule.CreatedByUserId,
                CreatedAt = bulkSchedule.CreatedAt,
                UpdatedAt = bulkSchedule.UpdatedAt,
                Payments = payments.Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    InvoiceNumber = p.InvoiceNumber,
                    GrossAmount = p.GrossAmount,
                    VatAmount = p.VatAmount,
                    WhtAmount = p.WhtAmount,
                    NetAmount = p.NetAmount,
                    Description = p.Description,
                    Reference = p.Reference,
                    InvoiceDate = p.InvoiceDate,
                    DueDate = p.DueDate,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    Remarks = p.Remarks,
                    VendorId = p.VendorId,
                    BulkScheduleId = p.BulkScheduleId,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList()
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while generating bulk schedule: {ex.Message}");
        }
    }

    public async Task<Result<List<BulkScheduleListResponse>>> GetBulkSchedulesForCurrentTenantAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedules = await _context.BulkSchedules
                .Include(bs => bs.CreatedByUser)
                .Where(bs => bs.TenantId == TenantId)
                .OrderByDescending(bs => bs.CreatedAt)
                .Select(bs => new BulkScheduleListResponse
                {
                    Id = bs.Id,
                    BatchNumber = bs.BatchNumber,
                    Description = bs.Description,
                    TotalGrossAmount = bs.TotalGrossAmount,
                    TotalVatAmount = bs.TotalVatAmount,
                    TotalWhtAmount = bs.TotalWhtAmount,
                    TotalNetAmount = bs.TotalNetAmount,
                    PaymentCount = bs.PaymentCount,
                    ScheduledDate = bs.ScheduledDate,
                    Status = bs.Status,
                    CreatedAt = bs.CreatedAt
                })
                .ToListAsync();

            return Result.Ok(bulkSchedules);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving bulk schedules: {ex.Message}");
        }
    }

    public async Task<Result<BulkScheduleResponse>> GetBulkScheduleByIdAsync(string bulkScheduleId)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.CreatedByUser)
                .FirstOrDefaultAsync(bs => bs.Id == bulkScheduleId && bs.TenantId == TenantId);

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            // Get payments linked to this bulk schedule
            var payments = await _context.Payments
                .Include(p => p.Vendor)
                .ThenInclude(v => v.Bank)
                .Where(p => p.BulkScheduleId == bulkScheduleId && p.TenantId == TenantId)
                .OrderBy(p => p.Vendor.Name)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync();

            var response = new BulkScheduleResponse
            {
                Id = bulkSchedule.Id,
                BatchNumber = bulkSchedule.BatchNumber,
                Description = bulkSchedule.Description,
                TotalGrossAmount = bulkSchedule.TotalGrossAmount,
                TotalVatAmount = bulkSchedule.TotalVatAmount,
                TotalWhtAmount = bulkSchedule.TotalWhtAmount,
                TotalNetAmount = bulkSchedule.TotalNetAmount,
                PaymentCount = bulkSchedule.PaymentCount,
                ScheduledDate = bulkSchedule.ScheduledDate,
                ProcessedDate = bulkSchedule.ProcessedDate,
                Status = bulkSchedule.Status,
                Remarks = bulkSchedule.Remarks,
                CreatedByUserId = bulkSchedule.CreatedByUserId,
                CreatedAt = bulkSchedule.CreatedAt,
                UpdatedAt = bulkSchedule.UpdatedAt,
                Payments = payments.Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    InvoiceNumber = p.InvoiceNumber,
                    GrossAmount = p.GrossAmount,
                    VatAmount = p.VatAmount,
                    WhtAmount = p.WhtAmount,
                    NetAmount = p.NetAmount,
                    Description = p.Description,
                    Reference = p.Reference,
                    InvoiceDate = p.InvoiceDate,
                    DueDate = p.DueDate,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status,
                    Remarks = p.Remarks,
                    VendorId = p.VendorId,
                    BulkScheduleId = p.BulkScheduleId,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList()
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving the bulk schedule: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateBulkScheduleStatusAsync(string bulkScheduleId, UpdateBulkScheduleStatusRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.CreatedByUser)
                .FirstOrDefaultAsync(bs => bs.Id == bulkScheduleId
                                    && bs.CreatedByUser.TenantUsers.Any(tu => tu.TenantId == TenantId));

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            var validStatuses = new[] { "Draft", "Pending", "Approved", "Processed", "Completed", "Rejected", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return Result.Fail($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

            bulkSchedule.Status = request.Status;
            bulkSchedule.Remarks = request.Remarks ?? bulkSchedule.Remarks;
            bulkSchedule.UpdatedBy = UserId;
            bulkSchedule.UpdatedAt = DateTime.UtcNow;

            if (request.Status == "Processed" && !bulkSchedule.ProcessedDate.HasValue)
            {
                bulkSchedule.ProcessedDate = DateTime.UtcNow;
                bulkSchedule.ProcessedByUserId = UserId;
            }

            await _context.SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while updating bulk schedule status: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteBulkScheduleAsync(string bulkScheduleId)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.Payments)
                .FirstOrDefaultAsync(bs => bs.Id == bulkScheduleId && bs.TenantId == TenantId);

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            if (bulkSchedule.Status != "Draft")
                return Result.Fail("Only bulk schedules with 'Draft' status can be deleted");

            foreach (var payment in bulkSchedule.Payments)
            {
                payment.BulkScheduleId = null;
            }

            _context.BulkSchedules.Remove(bulkSchedule);
            await _context.SaveChangesAsync();

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while deleting bulk schedule: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApproveBulkScheduleAsync(string bulkScheduleId, ApproveBulkScheduleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.CreatedByUser)
                .Include(bs => bs.Payments)
                .FirstOrDefaultAsync(bs => bs.Id == bulkScheduleId && bs.TenantId == TenantId);

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            if (bulkSchedule.Status.ToLower() != "pending")
                return Result.Fail($"Cannot approve bulk schedule in {bulkSchedule.Status} status. Only pending bulk schedules can be approved.");

            bulkSchedule.Status = "Approved";
            bulkSchedule.Remarks = request.Remarks ?? bulkSchedule.Remarks;
            bulkSchedule.UpdatedBy = UserId;
            bulkSchedule.UpdatedAt = DateTime.UtcNow;
            bulkSchedule.ApprovedByUserId = UserId;
            bulkSchedule.ApprovedDate = DateTime.UtcNow;

            foreach (var payment in bulkSchedule.Payments)
            {
                payment.Status = "Scheduled";
                payment.BulkScheduleId = bulkSchedule.Id; 
                payment.UpdatedBy = UserId;
                payment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while approving bulk schedule: {ex.Message}");
        }
    }

    public async Task<Result<BulkScheduleExportResponse>> ExportBulkScheduleToCsvAsync(string bulkScheduleId)
    {
        try
        {
            if (string.IsNullOrEmpty(TenantId))
                return Result.Fail("User is not associated with any tenant");

            var bulkSchedule = await _context.BulkSchedules
                .Include(bs => bs.CreatedByUser)
                .Include(bs => bs.Payments)
                .ThenInclude(p => p.Vendor)
                .ThenInclude(v => v.Bank)
                .FirstOrDefaultAsync(bs => bs.Id == bulkScheduleId
                                    && bs.TenantId == TenantId);

            if (bulkSchedule == null)
                return Result.Fail("Bulk schedule not found");

            if (bulkSchedule.Status.ToLower() != "approved")
                return Result.Fail("Only approved bulk schedules can be exported");

            var csv = new StringBuilder();

            csv.AppendLine("Account Details,Amount,VAT (7.5%),WHT (2%),Amount Payable,Narration");

            foreach (var payment in bulkSchedule.Payments.OrderBy(p => p.Vendor.Name))
            {
                var accountDetails = $"{payment.Vendor.Name}\n{payment.Vendor.Bank?.Name ?? "N/A"}\n{payment.Vendor.Bank?.Code ?? "N/A"}";
                var vatAmount = payment.VatAmount > 0 ? payment.VatAmount.ToString("N2") : "N/A";
                var whtAmount = payment.WhtAmount > 0 ? payment.WhtAmount.ToString("N2") : "N/A";

                csv.AppendLine($"\"{accountDetails}\",{payment.GrossAmount:N2},{vatAmount},{whtAmount},{payment.NetAmount:N2},\"{payment.Description}\"");
            }

            csv.AppendLine("");
            csv.AppendLine("Summary");
            csv.AppendLine($"Total Amount,{bulkSchedule.TotalGrossAmount:N2}");
            csv.AppendLine($"Total VAT,{bulkSchedule.TotalVatAmount:N2}");
            csv.AppendLine($"Total WHT,{bulkSchedule.TotalWhtAmount:N2}");
            csv.AppendLine($"Total Payable,{bulkSchedule.TotalNetAmount:N2}");

            var response = new BulkScheduleExportResponse
            {
                FileContent = Encoding.UTF8.GetBytes(csv.ToString()),
                FileName = $"BulkSchedule_{bulkSchedule.BatchNumber}_{DateTime.UtcNow:yyyyMMdd}.csv",
                ContentType = "text/csv"
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while exporting bulk schedule: {ex.Message}");
        }
    }

    private async Task<string> GenerateBatchNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"BS{today:yyyyMMdd}";
        
        var lastBatchNumber = await _context.BulkSchedules
            .Where(bs => bs.BatchNumber.StartsWith(prefix))
            .OrderByDescending(bs => bs.BatchNumber)
            .Select(bs => bs.BatchNumber)
            .FirstOrDefaultAsync();

        if (lastBatchNumber == null)
        {
            return $"{prefix}001";
        }

        var lastSequence = lastBatchNumber.Substring(prefix.Length);
        if (int.TryParse(lastSequence, out var sequenceNumber))
        {
            return $"{prefix}{(sequenceNumber + 1):D3}";
        }

        return $"{prefix}001";
    }
}