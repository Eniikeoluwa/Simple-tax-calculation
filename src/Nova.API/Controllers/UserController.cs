using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nova.API.Application.Actions.Queries.User;
using Nova.API.Controllers;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    public UserController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var query = new GetUsersQuery();
        return await SendQuery<GetUsersQuery, List<UserResponse>>(query);
    }
}
