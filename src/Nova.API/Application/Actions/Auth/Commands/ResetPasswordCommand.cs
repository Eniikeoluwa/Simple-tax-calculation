using FluentResults;
using Nova.API.Application.Services.Common;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using FluentValidation;

namespace Nova.API.Application.Actions.Auth.Commands;

public record ResetPasswordCommand(ResetPasswordRequest request) : IRequest<ResetPasswordResponse>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var validateResult = await _authService.ValidatePasswordResetTokenAsync(request.request);
        if (validateResult.IsFailed)
            return Result.Fail(validateResult.Errors);

        var user = validateResult.Value;
        var updateResult = await _authService.UpdatePasswordAsync(user.Id, request.request.NewPassword);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);

        return Result.Ok();
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.request.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.request.Token).NotEmpty().WithMessage("Reset token is required.");
        RuleFor(x => x.request.NewPassword).NotEmpty().WithMessage("New password is required.").MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
