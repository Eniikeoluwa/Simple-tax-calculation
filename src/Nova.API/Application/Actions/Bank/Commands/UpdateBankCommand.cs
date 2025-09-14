using FluentResults;
using FluentValidation;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using Nova.Domain.Entities;

namespace Nova.API.Application.Actions.Bank.Commands;

public record UpdateBankCommand(UpdateBankRequest request) : IRequest<Result<BankResponse>>;

public class UpdateBankCommandHandler : IRequestHandler<UpdateBankCommand, Result<BankResponse>>
{
    private readonly IBankService _bankService;

    public UpdateBankCommandHandler(IBankService bankService)
    {
        _bankService = bankService;
    }

    public async Task<Result<BankResponse>> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        // First, get the existing bank
        var existingBankResult = await _bankService.GetBankByIdAsync(request.request.Id);
        if (existingBankResult.IsFailed)
            return Result.Fail(existingBankResult.Errors);

        var existingBank = existingBankResult.Value;

        // Update the bank properties
        var updatedBank = new Nova.Domain.Entities.Bank
        {
            Id = existingBank.Id,
            Name = request.request.Name,
            SortCode = request.request.SortCode,
            Code = request.request.Code,
            IsActive = request.request.IsActive,
            CreatedAt = existingBank.CreatedAt,
            CreatedBy = existingBank.CreatedBy
        };

        var updateResult = await _bankService.UpdateBankAsync(updatedBank);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);

        // Get the updated bank to return in response
        var updatedBankResult = await _bankService.GetBankByIdAsync(request.request.Id);
        if (updatedBankResult.IsFailed)
            return Result.Fail(updatedBankResult.Errors);

        var bank = updatedBankResult.Value;

        var response = new BankResponse
        {
            Id = bank.Id,
            Name = bank.Name,
            SortCode = bank.SortCode,
            Code = bank.Code,
            IsActive = bank.IsActive,
            CreatedAt = bank.CreatedAt,
            UpdatedAt = bank.UpdatedAt,
            CreatedBy = bank.CreatedBy,
            UpdatedBy = bank.UpdatedBy
        };

        return Result.Ok(response);
    }
}
public class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.request.Id)
            .NotEmpty()
            .WithMessage("Bank ID is required");

        RuleFor(x => x.request.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Bank name is required and must not exceed 200 characters");

        RuleFor(x => x.request.SortCode)
            .MaximumLength(50)
            .WithMessage("Bank sort code must not exceed 50 characters");

        RuleFor(x => x.request.Code)
            .MaximumLength(50)
            .WithMessage("Bank code must not exceed 50 characters");

        // Ensure at least sort code or code is provided for uniqueness
        RuleFor(x => x.request)
            .Must(x => !string.IsNullOrEmpty(x.SortCode) || !string.IsNullOrEmpty(x.Code))
            .WithMessage("Either sort code or bank code must be provided");
    }
}
