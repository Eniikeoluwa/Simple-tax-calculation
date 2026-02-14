using FluentResults;
using Nova.API.Application.Services.Common;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using FluentValidation;

namespace Nova.API.Application.Actions.Auth.Commands;

public record RefreshTokenCommand(RefreshTokenRequest request) : MediatR.IRequest<Result<TokenResponse>>;

public class RefreshTokenCommandHandler : MediatR.IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
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
        var tokenResult = await _authService.GetRefreshTokenAsync(request.request);
        if (tokenResult.IsFailed)
            return Result.Fail(tokenResult.Errors);

        var existingToken = tokenResult.Value;
        if (existingToken.IsExpired || existingToken.IsRevoked)
            return Result.Fail("Invalid refresh token");

        var user = existingToken.User;
        if (user == null)
            return Result.Fail("Associated user not found");

        await _authService.RevokeRefreshTokenAsync(existingToken.Token);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _tokenService.GetRefreshTokenExpiryDate();

        var createResult = await _authService.CreateRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry);
        if (createResult.IsFailed)
            return Result.Fail(createResult.Errors);

        var response = new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        return Result.Ok(response);
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.request.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}