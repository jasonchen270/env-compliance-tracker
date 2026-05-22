using System.Security.Claims;
using EnvComplianceTracker.Infrastructure;

namespace EnvComplianceTracker.Api;

public class CurrentUserAccessor : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUserAccessor(IHttpContextAccessor accessor) => _accessor = accessor;
    public string? Username => _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
}
