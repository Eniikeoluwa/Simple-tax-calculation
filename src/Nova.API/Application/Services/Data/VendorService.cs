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
    Task<Result<Vendor>> UpdateVendorAsync(string vendorId, UpdateVendorRequest request);
    Task<Result<bool>> DeleteVendorAsync(string vendorId);
}

public class VendorService : BaseDataService, IVendorService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IBankService _bankService;

    public VendorService(AppDbContext context, ICurrentUserService currentUserService, IBankService bankService) : base(context)
    {
        _currentUserService = currentUserService;
        _bankService = bankService;
    }

    public async Task<Result<Vendor>> CreateVendorAsync(CreateVendorRequest request)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var currentUser = _currentUserService.UserId;

            var existingVendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Name.ToLower() == request.Name.ToLower() && v.TenantId == tenantId);

            if (existingVendor != null)
                return Result.Fail("A vendor with this name already exists in this tenant");

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result.Fail("Tenant not found");

            string bankId = request.BankId;
            
            if (!string.IsNullOrEmpty(request.BankName))
            {
                var bankCreateRequest = new CreateBankRequest
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
                var bankResult = await _bankService.GetBankByIdAsync(request.BankId);
                if (bankResult.IsFailed)
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
            var tenantId = _currentUserService.TenantId;
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
            var tenantId = _currentUserService.TenantId;
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

    public async Task<Result<Vendor>> UpdateVendorAsync(string vendorId, UpdateVendorRequest request)
    {
        try
        {
            var existingVendor = await _context.Vendors.Include(v => v.Bank).FirstOrDefaultAsync(v => v.Id == vendorId);
            if (existingVendor == null)
                return Result.Fail("Vendor not found");

            var tenantId = _currentUserService.TenantId;
            if (!string.IsNullOrEmpty(tenantId) && existingVendor.TenantId != tenantId)
                return Result.Fail("Vendor not found in current tenant");

            if (!string.IsNullOrEmpty(request.Name))
                existingVendor.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Code))
                existingVendor.Code = request.Code;
            if (!string.IsNullOrEmpty(request.AccountName))
                existingVendor.AccountName = request.AccountName;
            if (!string.IsNullOrEmpty(request.AccountNumber))
                existingVendor.AccountNumber = request.AccountNumber;
            if (!string.IsNullOrEmpty(request.Address))
                existingVendor.Address = request.Address;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                existingVendor.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.Email))
                existingVendor.Email = request.Email;
            if (!string.IsNullOrEmpty(request.TaxIdentificationNumber))
                existingVendor.TaxIdentificationNumber = request.TaxIdentificationNumber;
            if (!string.IsNullOrEmpty(request.TaxType))
                existingVendor.TaxType = request.TaxType;
            if (request.VatRate.HasValue)
                existingVendor.VatRate = request.VatRate.Value;
            if (request.WhtRate.HasValue)
                existingVendor.WhtRate = request.WhtRate.Value;
            if (!string.IsNullOrEmpty(request.BankId))
                existingVendor.BankId = request.BankId;
            if (request.IsActive.HasValue)
                existingVendor.IsActive = request.IsActive.Value;

            if (string.IsNullOrEmpty(request.BankId) && !string.IsNullOrEmpty(request.BankName))
            {
                var bankCreateRequest = new CreateBankRequest
                {
                    Name = request.BankName,
                    SortCode = request.BankSortCode ?? string.Empty,
                    Code = request.BankCode ?? string.Empty
                };
                var bankResult = await _bankService.FindOrCreateBankAsync(bankCreateRequest);
                if (bankResult.IsSuccess)
                {
                    existingVendor.BankId = bankResult.Value.Id;
                }
            }

            existingVendor.UpdatedAt = DateTime.UtcNow;
            existingVendor.UpdatedBy = _currentUserService.UserId;

            await _context.SaveChangesAsync();
            return Result.Ok(existingVendor);
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

            var tenantId = _currentUserService.TenantId;
            if (!string.IsNullOrEmpty(tenantId) && vendor.TenantId != tenantId)
                return Result.Fail("Vendor not found in current tenant");

            vendor.IsActive = false;
            vendor.UpdatedAt = DateTime.UtcNow;
            vendor.UpdatedBy = _currentUserService.UserId;

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
