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

    public string TenantId
    {
        get
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user == null) return "";
            
            // Debug: Log all claims
            foreach (var claim in user.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
            
            return user.GetPrimaryTenantId() ?? "";
        }
    }
    
    public string UserId =>
        _httpContextAccessor?.HttpContext?.User?.GetUserId() ?? "";

    public string Firstname =>
        _httpContextAccessor?.HttpContext?.User?.GetFirstName() ?? "";

    public string Lastname =>
        _httpContextAccessor?.HttpContext?.User?.GetLastName() ?? "";
}