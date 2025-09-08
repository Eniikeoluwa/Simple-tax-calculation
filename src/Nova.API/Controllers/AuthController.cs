using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Auth.Commands;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignupRequest request)
    {
        var command = new SignupCommand(request);
        return await SendCommand<SignupCommand, AuthResponse>(command);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        return await SendCommand<LoginCommand, AuthResponse>(command);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request);
        return await SendCommand<RefreshTokenCommand, TokenResponse>(command);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request);
        return await SendCommand<ForgotPasswordCommand, ForgotPasswordResponse>(command);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request);
        return await SendCommand<ResetPasswordCommand, ResetPasswordResponse>(command);
    }
}