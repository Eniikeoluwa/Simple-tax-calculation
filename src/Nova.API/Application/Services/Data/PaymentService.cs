using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.API.Application.Services.Common;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;

namespace Nova.API.Application.Services.Data;

public interface IPaymentService
{
    Task<Result<Payment>> CreatePaymentAsync(CreatePaymentRequest request);
    Task<Result<List<Payment>>> GetPaymentsForCurrentTenantAsync();
    Task<Result<Payment>> GetPaymentByIdAsync(string paymentId);
    Task<Result<bool>> UpdatePaymentStatusAsync(string paymentId, UpdatePaymentStatusRequest request);
    Task<Result<bool>> DeletePaymentAsync(string paymentId);
}

public class PaymentService : BaseDataService, IPaymentService
{
    private readonly string _tenantId;
    private readonly string _userId;
    private readonly IVendorService _vendorService;

    public PaymentService(AppDbContext context, IVendorService vendorService) : base(context)
    {
        _tenantId = CurrentUser.TenantId;
        _userId = CurrentUser.UserId;
        _vendorService = vendorService;
    }

    public async Task<Result<Payment>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var currentUser = _userId;
            if (string.IsNullOrEmpty(currentUser))
                return Result.Fail("User not authenticated");

            // Check if invoice number already exists for this tenant
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.InvoiceNumber.ToLower() == request.InvoiceNumber.ToLower() 
                                    && p.CreatedByUser.TenantUsers.Any(tu => tu.TenantId == tenantId));

            if (existingPayment != null)
                return Result.Fail($"A payment with invoice number '{request.InvoiceNumber}' already exists");

            // Get vendor and validate it belongs to the current tenant
            var vendorResult = await _vendorService.GetVendorByIdAsync(request.VendorId);
            if (vendorResult.IsFailed)
                return Result.Fail($"Vendor not found: {request.VendorId}");

            var vendor = vendorResult.Value;
            if (vendor.TenantId != tenantId)
                return Result.Fail("Vendor does not belong to your tenant");

            // Create payment with tax calculations based on vendor settings
            var payment = new Payment
            {
                InvoiceNumber = request.InvoiceNumber,
                GrossAmount = request.GrossAmount,
                Description = request.Description,
                Reference = request.Reference,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Remarks = request.Remarks,
                VendorId = request.VendorId,
                CreatedByUserId = currentUser,
                
                // Apply vendor's tax configuration
                AppliedTaxType = vendor.TaxType,
                AppliedVatRate = vendor.VatRate,
                AppliedWhtRate = vendor.WhtRate
            };

            // Calculate VAT, WHT, and Net Amount based on vendor settings
            payment.CalculateNetAmount();

            _context.Payments.Add(payment);
            await SaveChangesAsync();

            // Load related entities for the response
            var createdPayment = await _context.Payments
                .Include(p => p.Vendor)
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            return Result.Ok(createdPayment ?? payment);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while creating the payment: {ex.Message}");
        }
    }

    public async Task<Result<List<Payment>>> GetPaymentsForCurrentTenantAsync()
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var payments = await _context.Payments
                .Include(p => p.Vendor)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ApprovedByUser)
                .Where(p => p.CreatedByUser.TenantUsers.Any(tu => tu.TenantId == tenantId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Result.Ok(payments);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving payments: {ex.Message}");
        }
    }

    public async Task<Result<Payment>> GetPaymentByIdAsync(string paymentId)
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var payment = await _context.Payments
                .Include(p => p.Vendor)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ApprovedByUser)
                .FirstOrDefaultAsync(p => p.Id == paymentId 
                                    && p.CreatedByUser.TenantUsers.Any(tu => tu.TenantId == tenantId));

            if (payment == null)
                return Result.Fail("Payment not found or you don't have access to it");

            return Result.Ok(payment);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while retrieving the payment: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdatePaymentStatusAsync(string paymentId, UpdatePaymentStatusRequest request)
    {
        try
        {
            var tenantId = _tenantId;
            var currentUser = _userId;
            
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            if (string.IsNullOrEmpty(currentUser))
                return Result.Fail("User not authenticated");

            var paymentResult = await GetPaymentByIdAsync(paymentId);
            if (paymentResult.IsFailed)
                return Result.Fail(paymentResult.Errors);

            var payment = paymentResult.Value;

            // Validate status transition
            var validStatuses = new[] { "Pending", "Approved", "Processed", "Paid", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return Result.Fail($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

            // Update payment status
            payment.Status = request.Status;
            payment.Remarks = request.Remarks ?? payment.Remarks;
            payment.PaymentDate = request.PaymentDate ?? payment.PaymentDate;

            // Set approved by user if status is approved
            if (request.Status == "Approved" && string.IsNullOrEmpty(payment.ApprovedByUserId))
            {
                payment.ApprovedByUserId = currentUser;
            }

            await SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while updating payment status: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeletePaymentAsync(string paymentId)
    {
        try
        {
            var paymentResult = await GetPaymentByIdAsync(paymentId);
            if (paymentResult.IsFailed)
                return Result.Fail(paymentResult.Errors);

            var payment = paymentResult.Value;

            // Only allow deletion if payment is still pending
            if (payment.Status != "Pending")
                return Result.Fail("Only pending payments can be deleted");

            _context.Payments.Remove(payment);
            await SaveChangesAsync();

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while deleting the payment: {ex.Message}");
        }
    }
}
