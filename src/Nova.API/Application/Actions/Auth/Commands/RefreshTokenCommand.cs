using FluentResults;
using Nova.API.Application.Common;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using FluentValidation;

namespace Nova.API.Application.Actions.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var validator = new RefreshTokenCommandValidator();
        var validation = validator.Validate(request);
        if (!validation.IsValid)
            return Result.Fail(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var existingRtResult = await _authService.GetRefreshTokenAsync(request.request.RefreshToken);
        if (existingRtResult.IsFailed)
            return Result.Fail(existingRtResult.Errors);

        var existingRt = existingRtResult.Value;
        if (existingRt.IsExpired || existingRt.IsRevoked)
            return Result.Fail("Invalid refresh token");

        var user = existingRt.User;
        if (user == null)
            return Result.Fail("Associated user not found");

        // revoke old refresh token
        await _authService.RevokeRefreshTokenAsync(existingRt.Token);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _tokenService.GetRefreshTokenExpiryDate();

        var createRtResult = await _authService.CreateRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);
        if (createRtResult.IsFailed)
            return Result.Fail(createRtResult.Errors);

        return Result.Ok(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
    }
}
public record RefreshTokenCommand(RefreshTokenRequest request) : IRequest<Result<TokenResponse>>;

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.request.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
        }
    }