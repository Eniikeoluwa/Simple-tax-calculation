using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Nova.API.Application.Actions.Bank.Commands;
using Nova.API.Application.Actions.Bank.Queries;
using Nova.Contracts.Models;

namespace Nova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankController : BaseController
{
    public BankController(IMediator mediator) : base(mediator)
    {
    }
    [HttpPost("create")]
    public async Task<ActionResult<BankResponse>> CreateBank([FromBody] CreateBankRequest request)
    {
        var command = new CreateBankCommand(request);
        return await SendCommand<CreateBankCommand, BankResponse>(command);
    }

    [HttpGet]
    public async Task<ActionResult<GetBanksResponse>> GetBanks()
    {
        var query = new GetBanksQuery();
        return await SendQuery<GetBanksQuery, GetBanksResponse>(query);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankResponse>> GetBankById(string id)
    {
        var query = new GetBankByIdQuery(id);
        return await SendQuery<GetBankByIdQuery, BankResponse>(query);
    }
    [HttpPatch("{id}")]
    public async Task<ActionResult<BankResponse>> UpdateBank(string id, [FromBody] UpdateBankRequest request)
    {
        // Ensure the ID from route matches the request
        request.Id = id;
        
        var command = new UpdateBankCommand(request);
        return await SendCommand<UpdateBankCommand, BankResponse>(command);
    }
}
