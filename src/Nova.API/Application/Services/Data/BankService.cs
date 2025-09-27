using FluentResults;
using Microsoft.EntityFrameworkCore;
using Nova.API.Application.Services.Common;
using Nova.Domain.Entities;
using Nova.Infrastructure;
using Nova.Contracts.Models;

namespace Nova.API.Application.Services.Data;

public interface IBankService
{
    Task<Result<Bank>> CreateBankAsync(CreateBankRequest request);
    Task<Result<Bank>> GetBankByIdAsync(string bankId);
    Task<Result<Bank>> FindOrCreateBankAsync(CreateBankRequest request);
    Task<Result<List<Bank>>> GetAllBanksAsync();
    Task<Result<Bank>> UpdateBankAsync(UpdateBankRequest request);
}

public class BankService : BaseDataService, IBankService
{
    private readonly string _userId;

    public BankService(AppDbContext context) : base(context)
    {
        _userId = CurrentUser.UserId;
    }

    public async Task<Result<Bank>> CreateBankAsync(CreateBankRequest request)
    {
        try
        {
            // Check if bank with same name and sort code already exists
            var existingBank = await _context.Banks
                .FirstOrDefaultAsync(b => b.Name.ToLower() == request.Name.ToLower() 
                    && b.SortCode.ToLower() == request.SortCode.ToLower() 
                    && b.IsActive);

            if (existingBank != null)
                return Result.Ok(existingBank); // Return existing bank instead of failing

            var currentUser = _userId ?? "System";

            var bank = new Bank
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                SortCode = request.SortCode,
                Code = request.Code,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser,
                UpdatedBy = currentUser
            };

            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();

            return Result.Ok(bank);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create bank: {ex.Message}");
        }
    }

    public async Task<Result<Bank>> GetBankByIdAsync(string bankId)
    {
        try
        {
            var bank = await _context.Banks
                .FirstOrDefaultAsync(b => b.Id == bankId && b.IsActive);

            if (bank == null)
                return Result.Fail("Bank not found");

            return Result.Ok(bank);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get bank: {ex.Message}");
        }
    }

    public async Task<Result<Bank>> FindOrCreateBankAsync(CreateBankRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result.Fail("Bank name is required");

            // Try to find existing bank first
            var existingBank = await _context.Banks
                .FirstOrDefaultAsync(b => b.Name.ToLower() == request.Name.ToLower()
                    && b.SortCode.ToLower() == request.SortCode.ToLower()
                    && b.IsActive);

            if (existingBank != null)
                return Result.Ok(existingBank);

            // Create new bank if not found
            return await CreateBankAsync(request);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to find or create bank: {ex.Message}");
        }
    }

    public async Task<Result<List<Bank>>> GetAllBanksAsync()
    {
        try
        {
            var banks = await _context.Banks
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return Result.Ok(banks);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to get banks: {ex.Message}");
        }
    }

    public async Task<Result<Bank>> UpdateBankAsync(UpdateBankRequest request)
    {
        try
        {
            var existingBank = await _context.Banks.FindAsync(request.Id);
            if (existingBank == null)
                return Result.Fail("Bank not found");

            existingBank.Name = request.Name;
            existingBank.SortCode = request.SortCode;
            existingBank.Code = request.Code;
            existingBank.IsActive = request.IsActive;
            existingBank.UpdatedAt = DateTime.UtcNow;
            existingBank.UpdatedBy = _userId ?? "System";

            await _context.SaveChangesAsync();
            return Result.Ok(existingBank);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update bank: {ex.Message}");
        }
    }
}
