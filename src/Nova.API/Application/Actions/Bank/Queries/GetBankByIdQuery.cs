using FluentResults;
using FluentValidation;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Bank.Queries;

public record GetBankByIdQuery(string BankId) : IRequest<Result<BankResponse>>;

public class GetBankByIdQueryHandler : IRequestHandler<GetBankByIdQuery, Result<BankResponse>>
{
    private readonly IBankService _bankService;

    public GetBankByIdQueryHandler(IBankService bankService)
    {
        _bankService = bankService;
    }

    public async Task<Result<BankResponse>> Handle(GetBankByIdQuery request, CancellationToken cancellationToken)
    {
        var bankResult = await _bankService.GetBankByIdAsync(request.BankId);

        if (bankResult.IsFailed)
            return Result.Fail(bankResult.Errors);

        var bank = bankResult.Value;

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
public class GetBankByIdQueryValidator : AbstractValidator<GetBankByIdQuery>
{
    public GetBankByIdQueryValidator()
    {
        RuleFor(x => x.BankId)
            .NotEmpty()
            .WithMessage("Bank ID is required");
    }
}
