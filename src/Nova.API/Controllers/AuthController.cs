using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Common;
using Nova.API.Application.Actions.Auth.Commands;
using Nova.Contracts.Models;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> SignUp([
        FromServices] IFeatureAction<SignupCommand, AuthResponse> action,
        [FromBody] SignupRequest request)
    {
        var command = new SignupCommand(request);
        return await SendAction(action, command);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([
        FromServices] IFeatureAction<LoginCommand, AuthResponse> action,
        [FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request);
        return await SendAction(action, command);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([
        FromServices] IFeatureAction<RefreshTokenCommand, TokenResponse> action,
        [FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request);
        return await SendAction(action, command);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([
        FromServices] IFeatureAction<ForgotPasswordCommand, Result> action,
        [FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request);
        return await SendAction(action, command);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([
        FromServices] IFeatureAction<ResetPasswordCommand, Result> action,
        [FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request);
        return await SendAction(action, command);
    }
}
