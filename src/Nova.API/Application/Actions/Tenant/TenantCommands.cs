using FluentResults;
using MediatR;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Tenant;

namespace Nova.API.Application.Actions.Tenant;

public class CreateTenantCommand : IRequest<Result<TenantResponse>>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantResponse>>
{
    private readonly IAuthService _authService;

    public CreateTenantCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<TenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
    var tenantResult = await _authService.CreateTenantAsync(
            request.Name,
            request.Description,
            request.Address,
            request.PhoneNumber,
            request.Email
        );

        if (tenantResult.IsFailed)
            return Result.Fail(tenantResult.Errors);

    var tenant = tenantResult.Value;

    return Result.Ok(new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Description = tenant.Description,
            Code = tenant.Code,
            Address = tenant.Address,
            PhoneNumber = tenant.PhoneNumber,
            Email = tenant.Email,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        });
    }
}
