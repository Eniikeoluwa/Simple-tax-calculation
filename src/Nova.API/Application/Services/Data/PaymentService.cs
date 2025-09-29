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

            var vendorResult = await _vendorService.GetVendorByIdAsync(request.VendorId);
            if (vendorResult.IsFailed)
                return Result.Fail($"Vendor not found: {request.VendorId}");

            var vendor = vendorResult.Value;
            if (vendor.TenantId != tenantId)
                return Result.Fail("Vendor does not belong to your tenant");

            Payment payment;

            if (request.IsPartialPayment)
            {
                var partialPaymentResult = await CreatePartialPaymentAsync(request, vendor, currentUser);
                if (partialPaymentResult.IsFailed)
                    return partialPaymentResult;
                payment = partialPaymentResult.Value;
            }
            else
            {
                payment = await CreateFullPaymentAsync(request, vendor, currentUser, tenantId);
            }

            _context.Payments.Add(payment);
            await SaveChangesAsync();

            var createdPayment = await _context.Payments
                .Include(p => p.Vendor)
                .Include(p => p.CreatedByUser)
                .Include(p => p.ParentPayment)
                .Include(p => p.ChildPayments)
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            return Result.Ok(createdPayment ?? payment);
        }
        catch (Exception ex)
        {
            return Result.Fail($"An error occurred while creating the payment: {ex.Message}");
        }
    }

    private async Task<Result<Payment>> CreatePartialPaymentAsync(CreatePaymentRequest request, Vendor vendor, string currentUser)
    {
        // Validate partial payment percentage
        if (request.PartialPercentage <= 0 || request.PartialPercentage >= 100)
            return Result.Fail("Partial payment percentage must be between 1 and 99");

        if (!request.IsFinalPayment)
        {
            // First partial payment (50% or 70%)
            if (request.PartialPercentage != 50 && request.PartialPercentage != 70)
                return Result.Fail("First partial payment must be either 50% or 70%");

            // Check for duplicate invoice numbers
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.InvoiceNumber.ToLower() == request.InvoiceNumber.ToLower()
                                    && p.CreatedByUser.TenantUsers.Any(tu => tu.TenantId == _tenantId));

            if (existingPayment != null)
                return Result.Fail($"A payment with invoice number '{request.InvoiceNumber}' already exists");

            var payment = new Payment
            {
                InvoiceNumber = request.InvoiceNumber,
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

            // Set as first partial payment
            payment.SetAsFirstPartialPayment(request.GrossAmount, request.PartialPercentage);

            // Calculate net amount (no tax deductions for first payment)
            payment.CalculateNetAmount();

            return Result.Ok(payment);
        }
        else
        {
            // Final payment (remaining 30% or 50%)
            if (string.IsNullOrEmpty(request.FirstPaymentId))
                return Result.Fail("FirstPaymentId is required for final payments");

            var firstPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.FirstPaymentId);

            if (firstPayment == null)
                return Result.Fail("First payment not found");

            if (firstPayment.IsFinalPayment)
                return Result.Fail("The referenced payment is already marked as final");

            // Validate the percentage matches the remaining amount
            var expectedRemainingPercentage = 100 - (firstPayment.PaymentAmount / firstPayment.OriginalInvoiceAmount * 100);
            if (Math.Abs(request.PartialPercentage - expectedRemainingPercentage) > 0.01m)
                return Result.Fail($"Final payment percentage should be {expectedRemainingPercentage:F1}% (remaining amount)");

            var payment = new Payment
            {
                InvoiceNumber = request.InvoiceNumber + "-FINAL",
                Description = request.Description,
                Reference = request.Reference,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                Remarks = request.Remarks,
                VendorId = request.VendorId,
                CreatedByUserId = currentUser,
                ParentPaymentId = request.FirstPaymentId,

                // Apply vendor's tax configuration
                AppliedTaxType = vendor.TaxType,
                AppliedVatRate = vendor.VatRate,
                AppliedWhtRate = vendor.WhtRate
            };

            // Set as final payment
            payment.SetAsFinalPayment(firstPayment.OriginalInvoiceAmount, firstPayment.PaymentAmount);

            // Calculate net amount (with tax deductions based on original amount)
            payment.CalculateNetAmount();

            // Update first payment to reflect total paid
            firstPayment.TotalAmountPaid = firstPayment.OriginalInvoiceAmount;
            _context.Payments.Update(firstPayment);

            return Result.Ok(payment);
        }
    }

    private async Task<Payment> CreateFullPaymentAsync(CreatePaymentRequest request, Vendor vendor, string currentUser, string tenantId)
    {
        // Check if invoice number already exists for this tenant
        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.InvoiceNumber.ToLower() == request.InvoiceNumber.ToLower()
                                && p.CreatedByUser.TenantUsers.Any(tu => tu.TenantId == tenantId));

        if (existingPayment != null)
            throw new InvalidOperationException($"A payment with invoice number '{request.InvoiceNumber}' already exists");

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

            // For full payments, set original amount to gross amount
            OriginalInvoiceAmount = request.GrossAmount,
            PaymentAmount = request.GrossAmount,
            TotalAmountPaid = request.GrossAmount,

            // Apply vendor's tax configuration
            AppliedTaxType = vendor.TaxType,
            AppliedVatRate = vendor.VatRate,
            AppliedWhtRate = vendor.WhtRate
        };

        // Calculate VAT, WHT, and Net Amount based on vendor settings
        payment.CalculateNetAmount();

        return payment;
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
