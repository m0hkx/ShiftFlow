using System.Security.Claims;

namespace ShiftFlow.Api.Services;

// Reads the logged-in user's ids from the JWT claims.
// Both are Guid.Empty when the request has no valid token.
public class CurrentUser(IHttpContextAccessor accessor)
{
    public const string OrgIdClaim = "orgId";

    public Guid OrgId => ReadGuid(OrgIdClaim);

    public Guid UserId => ReadGuid(ClaimTypes.NameIdentifier);

    private Guid ReadGuid(string claimType)
    {
        var value = accessor.HttpContext?.User.FindFirstValue(claimType);

        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
