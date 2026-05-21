using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.UnitTests.Common;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public TokenResult GenerateTokens(string userId, string email, IEnumerable<string> roles)
        => new("fake.access.token", "fake-refresh-token",
               DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7));

    public string GenerateRefreshToken() => "fake-refresh-token";
}
