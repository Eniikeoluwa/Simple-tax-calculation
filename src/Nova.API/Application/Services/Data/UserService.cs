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
    private readonly ICurrentUserService _currentUserService;

    public UserService(AppDbContext context, ICurrentUserService currentUserService) : base(context)
    {
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<User>>> GetUsersForCurrentTenantAsync()
    {
        try
        {
            var tenantId = _currentUserService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var users = await _context.Users
                .Where(u => u.TenantId == tenantId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            return Result.Ok(users);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get users for tenant '{_currentUserService.TenantId}': {ex.Message}");
        }
    }

    public async Task<Result<User>> GetUserByIdAsync(string userId)
    {
        try
        {
            var tenantId = _currentUserService.TenantId;
            if (string.IsNullOrEmpty(tenantId))
                return Result.Fail("User is not associated with any tenant");

            var user = await _context.Users
                .Where(u => u.Id == userId && u.TenantId == tenantId)
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
