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
        // Delegate update logic to the service - handler should not construct or manipulate domain entities
        var updateResult = await _bankService.UpdateBankAsync(request.request);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);

        var bank = updateResult.Value;

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
