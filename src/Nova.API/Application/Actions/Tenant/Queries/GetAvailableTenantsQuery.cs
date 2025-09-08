using FluentResults;
using Nova.API.Application.Services.Data;
using Nova.Contracts.Models;
using MediatR;

namespace Nova.API.Application.Actions.Tenant.Queries;

public record GetAvailableTenantsQuery() : MediatR.IRequest<Result<List<TenantResponse>>>;

public class GetAvailableTenantsQueryHandler : MediatR.IRequestHandler<GetAvailableTenantsQuery, Result<List<TenantResponse>>>
{
    private readonly ITenantService _tenantService;

    public GetAvailableTenantsQueryHandler(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<Result<List<TenantResponse>>> Handle(GetAvailableTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenantsResult = await _tenantService.GetAllTenantsAsync();
        if (tenantsResult.IsFailed)
            return Result.Fail(tenantsResult.Errors);

        var tenants = tenantsResult.Value;
        
        var response = tenants
            .Where(t => t.IsActive) // Only return active tenants for selection
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Code = t.Code,
                Address = t.Address,
                PhoneNumber = t.PhoneNumber,
                Email = t.Email,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToList();

        return Result.Ok(response);
    }
}
