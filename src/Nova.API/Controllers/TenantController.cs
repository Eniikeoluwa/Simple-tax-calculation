using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Tenant;
using Nova.API.Extensions;
using Nova.Contracts.Tenant;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantController : BaseController
{
    private readonly IMediator _mediator;

    public TenantController(IMediator mediator, ICurrentUserService currentUser) : base(currentUser)
    {
        _mediator = mediator;
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

        var command = new CreateTenantCommand
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email
        };

        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
        }

        return Ok(result.Value);
    }
}
