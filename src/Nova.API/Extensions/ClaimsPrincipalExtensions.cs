using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Nova.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value 
               ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }

    public static string? GetFirstName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.GivenName)?.Value 
               ?? principal.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value;
    }

    public static string? GetLastName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Surname)?.Value 
               ?? principal.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;
    }

    public static string? GetPrimaryTenantId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("primary_tenant_id")?.Value;
    }

    public static List<string> GetTenantIds(this ClaimsPrincipal principal)
    {
        return principal.FindAll("tenant_id").Select(c => c.Value).ToList();
    }

    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll("role").Select(c => c.Value).ToList();
    }

    public static string? GetRoleForTenant(this ClaimsPrincipal principal, string tenantId)
    {
        var tenantIds = principal.FindAll("tenant_id").Select(c => c.Value).ToList();
        var roles = principal.FindAll("role").Select(c => c.Value).ToList();

        var index = tenantIds.IndexOf(tenantId);
        if (index >= 0 && index < roles.Count)
        {
            return roles[index];
        }

        return null;
    }

    public static bool HasTenantAccess(this ClaimsPrincipal principal, string tenantId)
    {
        return principal.FindAll("tenant_id").Any(c => c.Value == tenantId);
    }

    public static bool IsAdmin(this ClaimsPrincipal principal, string? tenantId = null)
    {
        if (tenantId == null)
        {
            // Check if user is admin in any tenant
            return principal.FindAll("role").Any(c => c.Value == "Admin");
        }

        // Check if user is admin in specific tenant
        var role = principal.GetRoleForTenant(tenantId);
        return role == "Admin";
    }
}
