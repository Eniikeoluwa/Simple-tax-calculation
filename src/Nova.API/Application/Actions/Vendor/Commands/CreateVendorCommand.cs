using FluentResults;
using FluentValidation;
using Nova.API.Application.Common;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Vendor;
using Nova.Domain.Entities;

namespace Nova.API.Application.Actions.Vendor;

public record CreateVendorCommand(CreateVendorRequest request) : IRequest<Result<VendorResponse>>;

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

public class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, Result<VendorResponse>>
{
    private readonly AppDbContext _context;
    private readonly IDateService _dateService;
    private readonly IValidator<CreateVendorCommand> _validator;

    public CreateVendorCommandHandler(AppDbContext context, IDateService dateService, IValidator<CreateVendorCommand> validator)
    {
        _context = context;
        _dateService = dateService;
        _validator = validator;
    }

    public async Task<Result<VendorResponse>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(string.Join(", ", errors));
        }

        // Check tenant exists
        var tenant = await _context.Tenants.FindAsync(new object[] { request.request.TenantId }, cancellationToken);
        if (tenant == null)
            return Result.Fail("Tenant not found");

        // Check duplicate vendor name within tenant
        var existing = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Name.ToLower() == request.request.Name.ToLower() && v.TenantId == request.request.TenantId, cancellationToken);

        if (existing != null)
            return Result.Fail("A vendor with this name already exists in this tenant");

        // Validate bank
        if (!string.IsNullOrEmpty(request.request.BankId))
        {
            var bank = await _context.Banks.FindAsync(new object[] { request.request.BankId }, cancellationToken);
            if (bank == null)
                return Result.Fail("Bank not found");
        }

        var vendor = new Domain.Entities.Vendor
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.request.Name,
            Code = request.request.Code,
            AccountName = request.request.AccountName,
            AccountNumber = request.request.AccountNumber,
            Address = request.request.Address,
            PhoneNumber = request.request.PhoneNumber,
            Email = request.request.Email,
            TaxIdentificationNumber = request.request.TaxIdentificationNumber,
            TaxType = request.request.TaxType,
            VatRate = request.request.VatRate,
            WhtRate = request.request.WhtRate,
            BankId = request.request.BankId,
            TenantId = request.request.TenantId,
            IsActive = true,
            CreatedAt = _dateService.UtcNow,
            UpdatedAt = _dateService.UtcNow
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

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
