using FluentResults;
using FluentValidation;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Vendor;
using Nova.Domain.Entities;

namespace Nova.API.Application.Actions.Vendor;

public class CreateVendorCommand : IRequest<Result<VendorResponse>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxIdentificationNumber { get; set; } = string.Empty;
    public string TaxType { get; set; } = "Both";
    public decimal VatRate { get; set; } = 7.5m;
    public decimal WhtRate { get; set; } = 2.0m;
    public string BankId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Vendor name is required and must not exceed 200 characters");

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Vendor code is required and must not exceed 50 characters");

        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Account name is required and must not exceed 200 characters");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Account number is required and must not exceed 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Please provide a valid email address");

        RuleFor(x => x.TaxType)
            .Must(x => new[] { "None", "VAT", "WHT", "Both" }.Contains(x))
            .WithMessage("Tax type must be one of: None, VAT, WHT, Both");

        RuleFor(x => x.VatRate)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("VAT rate must be between 0 and 100");

        RuleFor(x => x.WhtRate)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("WHT rate must be between 0 and 100");

        RuleFor(x => x.TenantId)
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
        var tenant = await _context.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant == null)
            return Result.Fail("Tenant not found");

        // Check duplicate vendor name within tenant
        var existing = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Name.ToLower() == request.Name.ToLower() && v.TenantId == request.TenantId, cancellationToken);

        if (existing != null)
            return Result.Fail("A vendor with this name already exists in this tenant");

        // Validate bank
        if (!string.IsNullOrEmpty(request.BankId))
        {
            var bank = await _context.Banks.FindAsync(new object[] { request.BankId }, cancellationToken);
            if (bank == null)
                return Result.Fail("Bank not found");
        }

        var vendor = new Domain.Entities.Vendor
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Code = request.Code,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            TaxIdentificationNumber = request.TaxIdentificationNumber,
            TaxType = request.TaxType,
            VatRate = request.VatRate,
            WhtRate = request.WhtRate,
            BankId = request.BankId,
            TenantId = request.TenantId,
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
