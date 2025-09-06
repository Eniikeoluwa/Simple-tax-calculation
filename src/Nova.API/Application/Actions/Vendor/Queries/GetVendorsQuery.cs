// using FluentResults;
// using FluentValidation;
// using MediatR;
// using Nova.API.Application.Services.Data;
// using Nova.Contracts.Models;

// namespace Nova.API.Application.Actions.Vendor;

// public class GetVendorsQuery : IRequest<Result<GetVendorsResponse>>
// {
//     public string TenantId { get; set; } = string.Empty;
// }

// public class GetVendorsQueryValidator : AbstractValidator<GetVendorsQuery>
// {
//     public GetVendorsQueryValidator()
//     {
//         RuleFor(x => x.TenantId)
//             .NotEmpty()
//             .WithMessage("Tenant ID is required");
//     }
// }

// public class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, Result<GetVendorsResponse>>
// {
//     private readonly AppDbContext _context;
//     private readonly IValidator<GetVendorsQuery> _validator;

//     public GetVendorsQueryHandler(AppDbContext context, IValidator<GetVendorsQuery> validator)
//     {
//         _context = context;
//         _validator = validator;
//     }

//     public async Task<Result<GetVendorsResponse>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
//     {
//         var validationResult = await _validator.ValidateAsync(request, cancellationToken);
//         if (!validationResult.IsValid)
//         {
//             var errors = validationResult.Errors.Select(e => e.ErrorMessage);
//             return Result.Fail(string.Join(", ", errors));
//         }

//         var tenant = await _context.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
//         if (tenant == null)
//             return Result.Fail("Tenant not found");

//         var vendors = await _context.Vendors
//             .Where(v => v.TenantId == request.TenantId && v.IsActive)
//             .OrderBy(v => v.Name)
//             .ToListAsync(cancellationToken);

//         var response = vendors.Select(v => new VendorResponse
//         {
//             Id = v.Id,
//             Name = v.Name,
//             Code = v.Code,
//             AccountName = v.AccountName,
//             AccountNumber = v.AccountNumber,
//             Address = v.Address,
//             PhoneNumber = v.PhoneNumber,
//             Email = v.Email,
//             TaxIdentificationNumber = v.TaxIdentificationNumber,
//             TaxType = v.TaxType,
//             VatRate = v.VatRate,
//             WhtRate = v.WhtRate,
//             IsActive = v.IsActive,
//             BankId = v.BankId,
//             TenantId = v.TenantId,
//             CreatedAt = v.CreatedAt
//         }).ToList();

//         return Result.Ok(new GetVendorsResponse { Vendors = response });
//     }
// }
