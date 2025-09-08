using Nova.API.Application.Services.Common;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using FluentResults;
using FluentValidation;

namespace Nova.API.Application.Actions.Auth.Commands
{
    public class SignupCommandHandler : MediatR.IRequestHandler<SignupCommand, Result<AuthResponse>>
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ITenantService _tenantService;

        public SignupCommandHandler(IAuthService authService, ITokenService tokenService, ITenantService tenantService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _tenantService = tenantService;
        }

        public async Task<Result<AuthResponse>> Handle(SignupCommand command, CancellationToken cancellationToken)
        {
            // Validate that the tenant exists and is active
            var tenantResult = await _tenantService.GetTenantByIdAsync(command.request.TenantId);
            if (tenantResult.IsFailed)
                return Result.Fail("Selected company/tenant not found");

            var tenant = tenantResult.Value;
            if (!tenant.IsActive)
                return Result.Fail("Selected company/tenant is not active");

            var userResult = await _authService.CreateUserAsync(command.request);
            if (userResult.IsFailed)
                return Result.Fail(userResult.Errors);

            var user = userResult.Value;
            var response = new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                TenantId = user.TenantId,
                // Add other fields as needed
            };
            return Result.Ok(response);
        }
    }

    public record SignupCommand(SignupRequest request) : MediatR.IRequest<Result<AuthResponse>>;

    public class SignupCommandValidator : AbstractValidator<SignupCommand>
    {
        public SignupCommandValidator()
        {
            RuleFor(x => x.request.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.request.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.request.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");

            RuleFor(x => x.request.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[0-9+() -]{6,20}$").WithMessage("Invalid phone number format")
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

            RuleFor(x => x.request.TenantId)
                .NotEmpty().WithMessage("Tenant selection is required");
        }
    }
}