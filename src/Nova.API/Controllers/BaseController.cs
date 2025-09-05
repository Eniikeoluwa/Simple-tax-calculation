using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Services.Common;

namespace Nova.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ICurrentUserService _currentUser;

    protected BaseController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    protected string? CurrentUserId => _currentUser.UserId;
    protected string? CurrentPrimaryTenantId => _currentUser.PrimaryTenantId;
    protected IEnumerable<string> CurrentTenantIds => _currentUser.TenantIds;
    protected IEnumerable<string> CurrentRoles => _currentUser.Roles;

    protected ActionResult UnauthorizedResponse(string message = "Unauthorized") => Unauthorized(new { message });
    protected ActionResult BadRequestResponse(string message) => BadRequest(new { message });
    protected ActionResult OkResponse<T>(T value) => Ok(value);
}
