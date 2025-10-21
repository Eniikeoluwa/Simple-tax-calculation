using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Vendor.Commands;
using Nova.API.Application.Actions.Vendor.Queries;
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

    [HttpGet("list")]
    public async Task<ActionResult<List<VendorResponse>>> GetVendors()
    {
        var query = new GetVendorsQuery();
        return await SendQuery<GetVendorsQuery, List<VendorResponse>>(query);
    }

    [HttpPatch("{vendorId}")]
    public async Task<ActionResult<VendorResponse>> UpdateVendor(
        [FromRoute] string vendorId,
        [FromBody] UpdateVendorRequest request)
    {
        var command = new UpdateVendorCommand(vendorId, request);
        return await SendCommand<UpdateVendorCommand, VendorResponse>(command);
    }
}

