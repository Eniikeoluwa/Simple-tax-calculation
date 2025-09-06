using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Tenant;
using Nova.API.Extensions;
using Nova.API.Application.Common;
using Nova.Contracts.Models;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<TenantResponse>> CreateTenant([
        FromServices] IFeatureAction<CreateTenantCommand, TenantResponse> action,
        [FromBody] CreateTenantRequest request)
    {
        // Ensure user is authenticated and is an admin
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new CreateTenantCommand(request);
        return await SendAction(action, command);
    }
}
