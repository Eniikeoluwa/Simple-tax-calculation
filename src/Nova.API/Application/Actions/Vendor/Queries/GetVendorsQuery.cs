using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Queries.Vendor;

public record GetVendorsQuery() : MediatR.IRequest<Result<List<VendorResponse>>>;

public class GetVendorsQueryHandler : MediatR.IRequestHandler<GetVendorsQuery, Result<List<VendorResponse>>>
{
    private readonly IVendorService _vendorService;

    public GetVendorsQueryHandler(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    public async Task<Result<List<VendorResponse>>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        var vendorsResult = await _vendorService.GetVendorsForCurrentTenantAsync();
        if (vendorsResult.IsFailed)
            return Result.Fail(vendorsResult.Errors);

        var vendors = vendorsResult.Value;
        var response = vendors.Select(v => new VendorResponse
        {
            Id = v.Id,
            Name = v.Name,
            Code = v.Code,
            AccountName = v.AccountName,
            AccountNumber = v.AccountNumber,
            Address = v.Address,
            PhoneNumber = v.PhoneNumber,
            Email = v.Email,
            TaxIdentificationNumber = v.TaxIdentificationNumber,
            TaxType = v.TaxType,
            VatRate = v.VatRate,
            WhtRate = v.WhtRate,
            IsActive = v.IsActive,
            BankId = v.BankId,
            TenantId = v.TenantId,
            CreatedAt = v.CreatedAt
        }).ToList();

        return Result.Ok(response);
    }
}
