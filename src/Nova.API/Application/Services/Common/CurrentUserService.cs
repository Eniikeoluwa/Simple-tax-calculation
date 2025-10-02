using Nova.API.Extensions;

namespace Nova.API.Application.Services.Common;

public interface ICurrentUserService
{
    string TenantId { get; }
    string UserId { get; }
    string Firstname { get; }
    string Lastname { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string TenantId =>
        _httpContextAccessor?.HttpContext?.User?.GetPrimaryTenantId() ?? "";
    
    public string UserId =>
        _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? "";

    public string Firstname =>
        _httpContextAccessor?.HttpContext?.User?.GetFirstName() ?? "";

    public string Lastname =>
        _httpContextAccessor?.HttpContext?.User?.GetLastName() ?? "";
}