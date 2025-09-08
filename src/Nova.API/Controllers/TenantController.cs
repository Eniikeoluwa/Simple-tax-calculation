using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Tenant;
using Nova.API.Extensions;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController : BaseController
{
    public TenantController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("create")]
    public async Task<ActionResult<TenantResponse>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        var command = new CreateTenantCommand(request);
        return await SendCommand<CreateTenantCommand, TenantResponse>(command);
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<TenantResponse>>> GetAvailableTenants()
    {
        var query = new GetAvailableTenantsQuery();
        return await SendQuery<GetAvailableTenantsQuery, List<TenantResponse>>(query);
    }
}