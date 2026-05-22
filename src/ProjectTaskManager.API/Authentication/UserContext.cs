using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Domain.Constants;

namespace ProjectTaskManager.API.Authentication;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public string? UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?.User.IsInRole(Roles.Admin) == true;
}
