using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Vendor;
using Nova.API.Controllers;
using Nova.API.Application.Services.Common;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorController : BaseController
{
    public VendorController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("create")]
    public async Task<ActionResult<VendorResponse>> CreateVendor(
        [FromBody] CreateVendorRequest request)
    {
        var command = new CreateVendorCommand(request);
        return await SendCommand<CreateVendorCommand, VendorResponse>(command);
    }

    // [HttpGet("tenant/{tenantId}")]
    // public async Task<IActionResult> GetVendorsByTenant(
    //     [FromServices] IFeatureAction<GetVendorsQuery, GetVendorsResponse> action,
    //     string tenantId)
    // {
    //     var query = new GetVendorsQuery { TenantId = tenantId };
    //     return await SendAction(action, query);
    // }
}

