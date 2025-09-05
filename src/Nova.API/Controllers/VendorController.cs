using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Vendor;
using Nova.API.Controllers;
using Nova.Contracts.Vendor;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorController : BaseController
{
    public VendorController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequest request)
    {
        var command = new CreateVendorCommand
        {
            Name = request.Name,
            Code = request.Code,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            TaxIdentificationNumber = request.TaxIdentificationNumber,
            TaxType = request.TaxType,
            VatRate = request.VatRate,
            WhtRate = request.WhtRate,
            BankId = request.BankId,
            TenantId = request.TenantId
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(result.Value);
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetVendorsByTenant(string tenantId)
    {
        var query = new GetVendorsQuery
        {
            TenantId = tenantId
        };

        var result = await _mediator.Send(query);

        if (result.IsFailed)
            return BadRequest(result.Errors.Select(e => e.Message));

        return Ok(result.Value);
    }
}
