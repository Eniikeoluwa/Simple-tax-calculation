using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Auth;
using Nova.Contracts.Auth;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator, ICurrentUserService currentUser) : base(currentUser)
    {
        _mediator = mediator;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignupRequest request)
    {
        var command = new SignupCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            TenantName = request.TenantName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return Unauthorized(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return Unauthorized(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
        }

        return Ok(result.Value);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
        }

        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand
        {
            Email = request.Email,
            Token = request.Token,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
        }

        return Ok(new { message = "Password has been reset successfully." });
    }
}
