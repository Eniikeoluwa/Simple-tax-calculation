using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.API.Application.Services.Common;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;

namespace Nova.API.Application.Services.Data;

public interface IVendorService
{
    Task<Result<Vendor>> CreateVendorAsync(CreateVendorRequest request);
    // Task<Result<List<Vendor>>> GetVendorsByTenantIdAsync(string tenantId);
    // Task<Result<Vendor>> GetVendorByIdAsync(string vendorId);
    Task<Result<bool>> UpdateVendorAsync(Vendor vendor);
    Task<Result<bool>> DeleteVendorAsync(string vendorId);
}

public class VendorService : BaseDataService, IVendorService
{
    public VendorService(AppDbContext context, IDateService dateService) : base(context, dateService)
    {
    }

    public async Task<Result<Vendor>> CreateVendorAsync(CreateVendorRequest request)
    {
        try
        {
            // Check if vendor with same name exists in the tenant
            var existingVendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Name.ToLower() == request.Name.ToLower() && v.TenantId == request.TenantId);

            if (existingVendor != null)
                return Result.Fail("A vendor with this name already exists in this tenant");

            // Validate tenant exists
            var tenant = await _context.Tenants.FindAsync(request.TenantId);
            if (tenant == null)
                return Result.Fail("Tenant not found");

            // Validate bank exists if provided
            if (!string.IsNullOrEmpty(request.BankId))
            {
                var bank = await _context.Banks.FindAsync(request.BankId);
                if (bank == null)
                    return Result.Fail("Bank not found");
            }

            var vendor = new Vendor
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Code = request.Code,
                AccountName = request.AccountName,
                AccountNumber = request.AccountNumber,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                TaxIdentificationNumber = request.TaxIdentificationNumber,
                TaxType = request.TaxType,
                VatRate = request.VatRate,
                WhtRate = request.WhtRate,
                BankId = request.BankId,
                TenantId = request.TenantId,
                IsActive = true,
                CreatedAt = _dateService.UtcNow,
                UpdatedAt = _dateService.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return Result.Ok(vendor);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create vendor: {ex.Message}");
        }
    }

    // public async Task<Result<List<Vendor>>> GetVendorsByTenantIdAsync(string tenantId)
    // {
    //     try
    //     {
    //         var vendors = await _context.Vendors
    //             .Include(v => v.Bank)
    //             .Include(v => v.Tenant)
    //             .Where(v => v.TenantId == tenantId && v.IsActive)
    //             .OrderBy(v => v.Name)
    //             .ToListAsync();

    //         return Result.Ok(vendors);
    //     }
    //     catch (Exception ex)
    //     {
    //         return Result.Fail($"Failed to get vendors: {ex.Message}");
    //     }
    // }

    // public async Task<Result<Vendor>> GetVendorByIdAsync(string vendorId)
    // {
    //     try
    //     {
    //         var vendor = await _context.Vendors
    //             .Include(v => v.Bank)
    //             .Include(v => v.Tenant)
    //             .FirstOrDefaultAsync(v => v.Id == vendorId);

    //         if (vendor == null)
    //             return Result.Fail("Vendor not found");

    //         return Result.Ok(vendor);
    //     }
    //     catch (Exception ex)
    //     {
    //         return Result.Fail($"Failed to get vendor: {ex.Message}");
    //     }
    // }

    public async Task<Result<bool>> UpdateVendorAsync(Vendor vendor)
    {
        try
        {
            var existingVendor = await _context.Vendors.FindAsync(vendor.Id);
            if (existingVendor == null)
                return Result.Fail("Vendor not found");

            // Update properties
            existingVendor.Name = vendor.Name;
            existingVendor.Code = vendor.Code;
            existingVendor.AccountName = vendor.AccountName;
            existingVendor.AccountNumber = vendor.AccountNumber;
            existingVendor.Address = vendor.Address;
            existingVendor.PhoneNumber = vendor.PhoneNumber;
            existingVendor.Email = vendor.Email;
            existingVendor.TaxIdentificationNumber = vendor.TaxIdentificationNumber;
            existingVendor.TaxType = vendor.TaxType;
            existingVendor.VatRate = vendor.VatRate;
            existingVendor.WhtRate = vendor.WhtRate;
            existingVendor.BankId = vendor.BankId;
            existingVendor.IsActive = vendor.IsActive;
            existingVendor.UpdatedAt = _dateService.UtcNow;

            await _context.SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update vendor: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteVendorAsync(string vendorId)
    {
        try
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null)
                return Result.Fail("Vendor not found");

            vendor.IsActive = false;
            vendor.UpdatedAt = _dateService.UtcNow;
            
            _context.Vendors.Update(vendor);
            await _context.SaveChangesAsync();

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete vendor: {ex.Message}");
        }
    }
}
