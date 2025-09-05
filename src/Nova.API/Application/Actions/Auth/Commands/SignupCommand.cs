using Nova.API.Application.Common;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Auth;
using FluentResults;
using FluentValidation;

namespace Nova.API.Application.Actions.Auth.Commands
{
    public class SignupCommandHandler : IRequestHandler<SignupCommand, AuthResponse>
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public SignupCommandHandler(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        public async Task<Result<AuthResponse>> Handle(SignupCommand command, CancellationToken cancellationToken)
        {
            return await _authService.CreateUserAsync(command.request);
        }
    }

    public record SignupCommand(SignupRequest request) : IRequest<AuthResponse>;

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
        }
    }
}