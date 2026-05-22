using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.Common.Interfaces;

public interface IJwtTokenService
{
    TokenResult GenerateTokens(string userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
