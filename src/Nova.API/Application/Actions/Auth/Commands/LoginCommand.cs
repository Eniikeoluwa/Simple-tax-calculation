using FluentResults;
using Nova.API.Application.Services.Common;
using Nova.Contracts.Models;
using FluentValidation;
using Nova.API.Application.Services.Common;

namespace Nova.API.Application.Actions.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validator = new LoginCommandValidator();
        var validation = validator.Validate(request);
        if (!validation.IsValid)
            return Result.Fail(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var userResult = await _authService.ValidateUserAsync(request.request);
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
public record LoginCommand(LoginRequest request) : IRequest<Result<AuthResponse>>;
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.request.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.request.Password).NotEmpty().WithMessage("Password is required.");
        }
    }