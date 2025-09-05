using BCrypt.Net;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.Domain.Entities;
using Nova.Infrastructure;

namespace Nova.API.Application.Services.Data;

public interface IAuthService
{
    Task<Result<User>> ValidateUserAsync(string email, string password);
    Task<Result<User>> CreateUserAsync(string firstName, string lastName, string email, string password, string phoneNumber = "");
    Task<Result<RefreshToken>> CreateRefreshTokenAsync(string userId, string token, DateTime expiresAt);
    Task<Result<RefreshToken>> GetRefreshTokenAsync(string token);
    Task<Result> RevokeRefreshTokenAsync(string token);
    Task<Result<User>> GetUserByEmailAsync(string email);
    Task<Result> CreatePasswordResetTokenAsync(string userId, string token, DateTime expiresAt);
    Task<Result<User>> ValidatePasswordResetTokenAsync(string email, string token);
    Task<Result> UpdatePasswordAsync(string userId, string newPassword);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class AuthService : IAuthService, BaseDataService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<User>> ValidateUserAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
        {
            return Result.Fail("Invalid email or password");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return Result.Fail("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Ok(user);
    }

    public async Task<Result<User>> CreateUserAsync(string firstName, string lastName, string email, string password, string phoneNumber = "")
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return Result.Fail("User with this email already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Result.Ok(user);
    }

    public async Task<Result<RefreshToken>> CreateRefreshTokenAsync(string userId, string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Result.Ok(refreshToken);
    }

    public async Task<Result<RefreshToken>> GetRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            return Result.Fail("Refresh token not found");
        }

        return Result.Ok(refreshToken);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            return Result.Fail("Refresh token not found");
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Ok();
    }


    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
        {
            return Result.Fail("User not found");
        }

        return Result.Ok(user);
    }

    public async Task<Result> CreatePasswordResetTokenAsync(string userId, string token, DateTime expiresAt)
    {
        // For simplicity, we'll store password reset tokens in the RefreshTokens table with a special prefix
        // In a production app, you might want a separate table
        var resetToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Token = $"RESET_{token}",
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        _context.RefreshTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result<User>> ValidatePasswordResetTokenAsync(string email, string token)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
        {
            return Result.Fail("User not found");
        }

        var resetToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id && rt.Token == $"RESET_{token}" && rt.RevokedAt == null);

        if (resetToken == null || resetToken.IsExpired)
        {
            return Result.Fail("Invalid or expired reset token");
        }

        return Result.Ok(user);
    }

    public async Task<Result> UpdatePasswordAsync(string userId, string newPassword)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Result.Fail("User not found");
        }

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Revoke all refresh tokens and reset tokens for this user
        var tokensToRevoke = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var tokenToRevoke in tokensToRevoke)
        {
            tokenToRevoke.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Result.Ok();
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    private static string GenerateTenantCode(string name)
    {
        var cleanName = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        var shortCode = cleanName.Length > 6 ? cleanName[..6] : cleanName;
        return $"{shortCode}_{Random.Shared.Next(1000, 9999)}";
    }
}
