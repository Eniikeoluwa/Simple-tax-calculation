
using FluentResults;
using MediatR;
using Nova.API.Application.Services.Common;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Auth;
using FluentValidation;

namespace Nova.API.Application.Actions.Auth;

public class SignupCommand : IRequest<Result<AuthResponse>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SignupCommandHandler : IRequestHandler<SignupCommand, Result<AuthResponse>>
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public SignupCommandHandler(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
    _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var validator = new SignupCommandValidator();
        var validation = validator.Validate(request);
        if (!validation.IsValid)
            return Result.Fail(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var userResult = await _authService.CreateUserAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.PhoneNumber
        );

        if (userResult.IsFailed)
            return Result.Fail(userResult.Errors);

        var user = userResult.Value;

    var accessToken = _tokenService.GenerateAccessToken(user);
    var refreshToken = _tokenService.GenerateRefreshToken();
    var refreshTokenExpiry = _tokenService.GetRefreshTokenExpiryDate();

        var refreshTokenResult = await _authService.CreateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);
        if (refreshTokenResult.IsFailed)
            return Result.Fail(refreshTokenResult.Errors);

        return Result.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            TenantId = user.TenantId
        });
    }
}

// Validator (inlined)
public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.").MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
