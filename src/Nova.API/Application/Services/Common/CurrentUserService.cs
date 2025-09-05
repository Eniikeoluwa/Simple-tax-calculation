using System.Security.Claims;
using Nova.API.Extensions;

namespace Nova.API.Application.Services.Common;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? PrimaryTenantId { get; }
    IEnumerable<string> TenantIds { get; }
    IEnumerable<string> Roles { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.GetUserId();

    public string? PrimaryTenantId => User?.GetPrimaryTenantId();

    public IEnumerable<string> TenantIds => User?.GetTenantIds() ?? Enumerable.Empty<string>();

    public IEnumerable<string> Roles => User?.GetRoles() ?? Enumerable.Empty<string>();
}
