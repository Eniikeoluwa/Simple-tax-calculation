using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Bank.Queries;

public record GetBanksQuery() : IRequest<Result<GetBanksResponse>>;

public class GetBanksQueryHandler : IRequestHandler<GetBanksQuery, Result<GetBanksResponse>>
{
    private readonly IBankService _bankService;

    public GetBanksQueryHandler(IBankService bankService)
    {
        _bankService = bankService;
    }

    public async Task<Result<GetBanksResponse>> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        var banksResult = await _bankService.GetAllBanksAsync();

        if (banksResult.IsFailed)
            return Result.Fail(banksResult.Errors);

        var banks = banksResult.Value;

        var response = new GetBanksResponse
        {
            Banks = banks.Select(bank => new BankResponse
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
            }).ToList()
        };

        return Result.Ok(response);
    }
}
