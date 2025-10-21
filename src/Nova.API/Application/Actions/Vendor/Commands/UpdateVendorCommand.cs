using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Common;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using Nova.Domain.Entities;

namespace Nova.API.Application.Actions.Vendor.Commands;

public record UpdateVendorCommand(string VendorId, UpdateVendorRequest request) : MediatR.IRequest<Result<VendorResponse>>;

public class UpdateVendorCommandHandler : MediatR.IRequestHandler<UpdateVendorCommand, Result<VendorResponse>>
{
    private readonly IVendorService _vendorService;

    public UpdateVendorCommandHandler(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    public async Task<Result<VendorResponse>> Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendorResult = await _vendorService.UpdateVendorAsync(request.VendorId, request.request);
        if (vendorResult.IsFailed)
            return Result.Fail(vendorResult.Errors);

        var vendor = vendorResult.Value;

        var response = new VendorResponse
        {
            Id = vendor.Id,
            Name = vendor.Name,
            Code = vendor.Code,
            AccountName = vendor.AccountName,
            AccountNumber = vendor.AccountNumber,
            Address = vendor.Address,
            PhoneNumber = vendor.PhoneNumber,
            Email = vendor.Email,
            TaxIdentificationNumber = vendor.TaxIdentificationNumber,
            TaxType = vendor.TaxType,
            VatRate = vendor.VatRate,
            WhtRate = vendor.WhtRate,
            IsActive = vendor.IsActive,
            BankId = vendor.BankId,
            TenantId = vendor.TenantId,
            CreatedAt = vendor.CreatedAt
        };

        if (vendor.Bank != null)
        {
            response.Bank = new BankInfo
            {
                Id = vendor.Bank.Id,
                Name = vendor.Bank.Name,
                SortCode = vendor.Bank.SortCode,
                Code = vendor.Bank.Code
            };
        }

        return Result.Ok(response);
    }
}

public class UpdateVendorCommandValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorCommandValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty()
            .WithMessage("Vendor ID is required");

        RuleFor(x => x.request.Name)
            .NotEmpty()
            .When(x => x.request.Name != null)
            .WithMessage("Name cannot be empty if provided");

        RuleFor(x => x.request.Code)
            .NotEmpty()
            .When(x => x.request.Code != null)
            .WithMessage("Code cannot be empty if provided");

        RuleFor(x => x.request.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.request.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.request.VatRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.request.VatRate.HasValue)
            .WithMessage("VAT rate must be greater than or equal to 0");

        RuleFor(x => x.request.WhtRate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.request.WhtRate.HasValue)
            .WithMessage("WHT rate must be greater than or equal to 0");
    }
}