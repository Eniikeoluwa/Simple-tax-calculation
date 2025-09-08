using Nova.API.Extensions;

namespace Nova.API.Application.Services.Common;

public static class CurrentUser
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    public static string TenantId =>
        _httpContextAccessor?.HttpContext?.User?.GetPrimaryTenantId() ?? "";
    
    public static string UserId =>
        _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? "";

    public static string Firstname =>
        _httpContextAccessor?.HttpContext?.User?.GetFirstName() ?? "";

    public static string Lastname =>
        _httpContextAccessor?.HttpContext?.User?.GetLastName() ?? "";
}
