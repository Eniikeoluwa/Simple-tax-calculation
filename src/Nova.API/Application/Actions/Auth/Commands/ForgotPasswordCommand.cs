
using FluentResults;
using Nova.API.Application.Common;
using FluentValidation;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public ForgotPasswordCommandHandler(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _authService.GetUserByEmailAsync(request.request);
        if (userResult.IsFailed)
            return Result.Fail(userResult.Errors);

        var user = userResult.Value;

        var resetToken = _tokenService.GeneratePasswordResetToken();
        var expiry = _tokenService.GetPasswordResetTokenExpiryDate();

        var createResetResult = await _authService.CreatePasswordResetTokenAsync(user.Id, resetToken, expiry);
        if (createResetResult.IsFailed)
            return Result.Fail(createResetResult.Errors);

        // In production you'd email the token. For now return success.
        return Result.Ok();
    }
}

public record ForgotPasswordCommand(ForgotPasswordRequest request) : IRequest<Result>;

    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }