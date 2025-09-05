using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Auth;

namespace Nova.API.Application.Actions.Auth;

public class ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var validateResult = await _authService.ValidatePasswordResetTokenAsync(request.Email, request.Token);
        if (validateResult.IsFailed)
            return Result.Fail(validateResult.Errors);

        var user = validateResult.Value;
        var updateResult = await _authService.UpdatePasswordAsync(user.Id, request.NewPassword);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);

        return Result.Ok();
    }
}

// Validator (inlined)
public class ResetPasswordCommandValidator : FluentValidation.AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Reset token is required.");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required.").MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
