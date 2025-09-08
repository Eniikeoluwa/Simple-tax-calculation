using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Tenant;
using Nova.API.Extensions;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController : BaseController
{
    public TenantController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<ActionResult<TenantResponse>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        // Ensure user is authenticated and is an admin
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new CreateTenantCommand(request);
        return await SendCommand<CreateTenantCommand, TenantResponse>(command);
    }
}