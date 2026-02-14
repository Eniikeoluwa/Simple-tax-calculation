using FluentResults;
using Nova.API.Application.Services.Common;
using MediatR;
using FluentValidation;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Auth.Commands;

public record ForgotPasswordCommand(ForgotPasswordRequest request) : MediatR.IRequest<Result<ForgotPasswordResponse>>;

public class ForgotPasswordCommandHandler : MediatR.IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public ForgotPasswordCommandHandler(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var userResult = await _authService.GetUserByEmailAsync(request.request);
        if (userResult.IsFailed)
            return Result.Fail(userResult.Errors);

        var user = userResult.Value;

        var resetToken = _tokenService.GeneratePasswordResetToken();
        var expiry = _tokenService.GetPasswordResetTokenExpiryDate();

        var createResult = await _authService.CreatePasswordResetTokenAsync(user.Id, resetToken, expiry);
        if (createResult.IsFailed)
            return Result.Fail(createResult.Errors);

        return Result.Ok();
    }
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.request.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}