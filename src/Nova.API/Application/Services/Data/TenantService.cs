using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.API.Application.Services.Common;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;

namespace Nova.API.Application.Services.Data;

public interface ITenantService
{
    Task<Result<Tenant>> CreateTenantAsync(CreateTenantRequest request, string? createdBy = null);
    Task<Result<Tenant>> GetTenantByIdAsync(string tenantId);
    Task<Result<List<Tenant>>> GetAllTenantsAsync();
    Task<Result<bool>> UpdateTenantAsync(Tenant tenant, string? updatedBy = null);
    Task<Result<bool>> DeleteTenantAsync(string tenantId, string? updatedBy = null);
}

public class TenantService : BaseDataService, ITenantService
{
    public TenantService(AppDbContext context) : base(context)
    {
    }

    public async Task<Result<Tenant>> CreateTenantAsync(CreateTenantRequest request, string? createdBy = null)
    {
        try
        {
            var creator = string.IsNullOrEmpty(createdBy) ? "Admin" : createdBy;

            var tenant = new Tenant
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Code = GenerateTenantCode(request.Name),
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = creator,
                UpdatedBy = creator
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return Result.Ok(tenant);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create tenant: {ex.Message}");
        }
    }

    public async Task<Result<Tenant>> GetTenantByIdAsync(string tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.TenantUsers)
                .ThenInclude(tu => tu.User)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return Result.Fail("Tenant not found");

            return Result.Ok(tenant);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get tenant: {ex.Message}");
        }
    }

    public async Task<Result<List<Tenant>>> GetAllTenantsAsync()
    {
        try
        {
            var tenants = await _context.Tenants
                .Include(t => t.TenantUsers)
                .ThenInclude(tu => tu.User)
                .Where(t => t.IsActive)
                .ToListAsync();

            return Result.Ok(tenants);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get tenants: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateTenantAsync(Tenant tenant, string? updatedBy = null)
    {
        try
        {
            var existingTenant = await _context.Tenants.FindAsync(tenant.Id);
            if (existingTenant == null)
                return Result.Fail("Tenant not found");
            var updater = string.IsNullOrEmpty(updatedBy) ? "Admin" : updatedBy;

            existingTenant.Name = tenant.Name;
            existingTenant.Description = tenant.Description;
            existingTenant.Address = tenant.Address;
            existingTenant.PhoneNumber = tenant.PhoneNumber;
            existingTenant.Email = tenant.Email;
            existingTenant.IsActive = tenant.IsActive;
            existingTenant.UpdatedAt = DateTime.UtcNow;
            existingTenant.UpdatedBy = updater;

            await _context.SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update tenant: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTenantAsync(string tenantId, string? updatedBy = null)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return Result.Fail("Tenant not found");

            var updater = string.IsNullOrEmpty(updatedBy) ? "Admin" : updatedBy;

            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.UpdatedBy = updater;

            await _context.SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete tenant: {ex.Message}");
        }
    }

    private string GenerateTenantCode(string name)
    {
        var cleanName = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        var code = cleanName.Length >= 3 ? cleanName.Substring(0, 3) : cleanName;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        return $"{code}{timestamp.Substring(timestamp.Length - 4)}";
    }
}
