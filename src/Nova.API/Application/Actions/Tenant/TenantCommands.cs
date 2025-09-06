using FluentResults;
using Nova.API.Application.Services.Common;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Tenant;

public record CreateTenantCommand(CreateTenantRequest request) : IRequest<Result<TenantResponse>>;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantResponse>>
{
    private readonly ITenantService _tenantService;

    public CreateTenantCommandHandler(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<Result<TenantResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantResult = await _tenantService.CreateTenantAsync(request.request);

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
