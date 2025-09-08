using FluentResults;
using Nova.API.Application.Services.Data;
using MediatR;
using Nova.Contracts.Models;

namespace Nova.API.Application.Actions.Tenant.Commands;

public record CreateTenantCommand(CreateTenantRequest request) : MediatR.IRequest<Result<TenantResponse>>;

public class CreateTenantCommandHandler : MediatR.IRequestHandler<CreateTenantCommand, Result<TenantResponse>>
{
    private readonly ITenantService _tenantService;

    public CreateTenantCommandHandler(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<Result<TenantResponse>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        var tenantResult = await _tenantService.CreateTenantAsync(command.request);
        if (tenantResult.IsFailed)
            return Result.Fail(tenantResult.Errors);

        var tenant = tenantResult.Value;
        var response = MapToTenantResponse(tenant);
        return Result.Ok(response);
    }

    private static TenantResponse MapToTenantResponse(Domain.Entities.Tenant tenant)
    {
        return new TenantResponse
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
        };
    }
}
