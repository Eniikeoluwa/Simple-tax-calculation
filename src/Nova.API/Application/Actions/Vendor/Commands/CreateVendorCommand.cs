using FluentResults;
using FluentValidation;
using Nova.API.Application.Services.Common;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using Nova.Domain.Entities;

namespace Nova.API.Application.Actions.Vendor;

public record CreateVendorCommand(CreateVendorRequest request) : MediatR.IRequest<Result<VendorResponse>>;

public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(x => x.request.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Vendor name is required and must not exceed 200 characters");

        RuleFor(x => x.request.Code)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Vendor code is required and must not exceed 50 characters");

        RuleFor(x => x.request.AccountName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Account name is required and must not exceed 200 characters");

        RuleFor(x => x.request.AccountNumber)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Account number is required and must not exceed 50 characters");

        RuleFor(x => x.request.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.request.Email))
            .WithMessage("Please provide a valid email address");

        RuleFor(x => x.request.TaxType)
            .Must(x => new[] { "None", "VAT", "WHT", "Both" }.Contains(x))
            .WithMessage("Tax type must be one of: None, VAT, WHT, Both");

        RuleFor(x => x.request.VatRate)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("VAT rate must be between 0 and 100");

        RuleFor(x => x.request.WhtRate)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("WHT rate must be between 0 and 100");

        RuleFor(x => x.request.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");
    }
}

public class CreateVendorCommandHandler : MediatR.IRequestHandler<CreateVendorCommand, Result<VendorResponse>>
{
    private readonly IVendorService _vendorService;

    public CreateVendorCommandHandler(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    public async Task<Result<VendorResponse>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendorResult = await _vendorService.CreateVendorAsync(request.request);
        if (vendorResult.IsFailed)
            return Result.Fail(vendorResult.Errors);

        var vendor = vendorResult.Value;

        return Result.Ok(new VendorResponse
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
        });
    }
}
