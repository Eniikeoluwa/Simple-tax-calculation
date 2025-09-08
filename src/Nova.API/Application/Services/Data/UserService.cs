using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.API.Application.Services.Common;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;

namespace Nova.API.Application.Services.Data;

public interface IUserService
{
    Task<Result<List<User>>> GetUsersForCurrentTenantAsync();
    Task<Result<User>> GetUserByIdAsync(string userId);
}

public class UserService : BaseDataService, IUserService
{
    private readonly string _tenantId;
    private readonly string _userId;

    public UserService(AppDbContext context) : base(context)
    {
        _tenantId = CurrentUser.TenantId;
        _userId = CurrentUser.UserId;
    }

    public async Task<Result<List<User>>> GetUsersForCurrentTenantAsync()
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var users = await _context.TenantUsers
                .Include(tu => tu.User)
                .Include(tu => tu.Tenant)
                .Where(tu => tu.TenantId == tenantId)
                .Select(tu => tu.User)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            return Result.Ok(users);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get users: {ex.Message}");
        }
    }

    public async Task<Result<User>> GetUserByIdAsync(string userId)
    {
        try
        {
            var tenantId = _tenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var user = await _context.TenantUsers
                .Include(tu => tu.User)
                .Include(tu => tu.Tenant)
                .Where(tu => tu.TenantId == tenantId && tu.UserId == userId)
                .Select(tu => tu.User)
                .FirstOrDefaultAsync();

            if (user == null)
                return Result.Fail("User not found in this tenant");

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get user: {ex.Message}");
        }
    }
}
