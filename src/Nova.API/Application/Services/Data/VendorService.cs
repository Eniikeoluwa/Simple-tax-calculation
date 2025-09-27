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
    Task<Result<List<Vendor>>> GetVendorsForCurrentTenantAsync();
    Task<Result<Vendor>> GetVendorByIdAsync(string vendorId);
    Task<Result<bool>> UpdateVendorAsync(Vendor vendor);
    Task<Result<bool>> DeleteVendorAsync(string vendorId);
}

public class VendorService : BaseDataService, IVendorService
{
    private readonly string _tenantId;
    private readonly string _userId;
    private readonly IBankService _bankService;

    public VendorService(AppDbContext context, IBankService bankService) : base(context)
    {
        _tenantId = CurrentUser.TenantId;
        _userId = CurrentUser.UserId;
        _bankService = bankService;
    }

    public async Task<Result<Vendor>> CreateVendorAsync(CreateVendorRequest request)
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var currentUser = _userId;

            // Check if vendor with same name exists in the tenant
            var existingVendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Name.ToLower() == request.Name.ToLower() && v.TenantId == tenantId);

            if (existingVendor != null)
                return Result.Fail("A vendor with this name already exists in this tenant");

            // Validate tenant exists
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result.Fail("Tenant not found");

            string bankId = request.BankId;
            
            // Handle bank creation or finding
            if (!string.IsNullOrEmpty(request.BankName))
            {
                // Create or find bank using the provided bank details
                var bankCreateRequest = new Nova.Contracts.Models.CreateBankRequest
                {
                    Name = request.BankName,
                    SortCode = request.BankSortCode,
                    Code = request.BankCode
                };
                var bankResult = await _bankService.FindOrCreateBankAsync(bankCreateRequest);
                
                if (bankResult.IsFailed)
                    return Result.Fail($"Failed to create or find bank: {string.Join(", ", bankResult.Errors.Select(e => e.Message))}");
                
                bankId = bankResult.Value.Id;
            }
            else if (!string.IsNullOrEmpty(request.BankId))
            {
                // Validate existing bank if BankId is provided
                var bankResult = await _bankService.GetBankByIdAsync(request.BankId);
                if (bankResult.IsFailed)
                    return Result.Fail("Bank not found");
            }
            // If neither BankId nor BankName is provided, bankId will remain null/empty

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
                BankId = bankId,
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser
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

    public async Task<Result<List<Vendor>>> GetVendorsForCurrentTenantAsync()
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var vendors = await _context.Vendors
                .Include(v => v.Bank)
                .Include(v => v.Tenant)
                .Where(v => v.TenantId == tenantId && v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync();

            return Result.Ok(vendors);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get vendors: {ex.Message}");
        }
    }

    public async Task<Result<Vendor>> GetVendorByIdAsync(string vendorId)
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var vendor = await _context.Vendors
                .Include(v => v.Bank)
                .Include(v => v.Tenant)
                .FirstOrDefaultAsync(v => v.Id == vendorId && v.TenantId == tenantId);

            if (vendor == null)
                return Result.Fail("Vendor not found");

            return Result.Ok(vendor);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get vendor: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateVendorAsync(Vendor vendor)
    {
        try
        {
            var existingVendor = await _context.Vendors.FindAsync(vendor.Id);
            if (existingVendor == null)
                return Result.Fail("Vendor not found");

            // Ensure vendor belongs to current tenant
            if (!string.IsNullOrEmpty(_tenantId) && existingVendor.TenantId != _tenantId)
                return Result.Fail("Vendor not found in current tenant");

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
            existingVendor.UpdatedAt = DateTime.UtcNow;
            existingVendor.UpdatedBy = _userId;

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

            if (!string.IsNullOrEmpty(_tenantId) && vendor.TenantId != _tenantId)
                return Result.Fail("Vendor not found in current tenant");

            vendor.IsActive = false;
            vendor.UpdatedAt = DateTime.UtcNow;
            vendor.UpdatedBy = _userId;

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
