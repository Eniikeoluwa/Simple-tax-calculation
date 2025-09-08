using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Queries.User;

public record GetUsersQuery() : MediatR.IRequest<Result<List<UserResponse>>>;

public class GetUsersQueryHandler : MediatR.IRequestHandler<GetUsersQuery, Result<List<UserResponse>>>
{
    private readonly IUserService _userService;

    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<List<UserResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var usersResult = await _userService.GetUsersForCurrentTenantAsync();
        if (usersResult.IsFailed)
            return Result.Fail(usersResult.Errors);

        var users = usersResult.Value;
        var response = users.Select(u => new UserResponse
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        return Result.Ok(response);
    }
}
