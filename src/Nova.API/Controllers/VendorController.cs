using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Vendor;
using Nova.API.Controllers;
using Nova.API.Application.Services.Common;
using Nova.Contracts.Models;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorController : BaseController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateVendor(
        [FromServices] IFeatureAction<CreateVendorCommand, VendorResponse> action,
        [FromBody] CreateVendorRequest request)
    {
        var command = new CreateVendorCommand(request);
        return await SendAction(action, command);
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetVendorsByTenant(
        [FromServices] IFeatureAction<GetVendorsQuery, GetVendorsResponse> action,
        string tenantId)
    {
        var query = new GetVendorsQuery { TenantId = tenantId };
        return await SendAction(action, query);
    }
}
