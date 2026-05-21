using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectTaskManager.Infrastructure.Authentication;

namespace ProjectTaskManager.Application.UnitTests.Infrastructure;

public sealed class JwtTokenServiceTests
{
    private static readonly JwtSettings TestSettings = new()
    {
        Secret = "test-secret-thats-at-least-32-characters-long-ok",
        Issuer = "test",
        Audience = "test",
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    };

    private static JwtTokenService CreateService() => new(Options.Create(TestSettings));

    [Fact]
    public void GenerateTokens_ReturnsValidJwtWithExpectedClaims()
    {
        const string UserId = "user-123";
        const string Email = "user@test.com";
        var roles = new[] { "User" };

        var tokenResult = CreateService().GenerateTokens(UserId, Email, roles);

        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = TestSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = TestSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSettings.Secret)),
            ValidateLifetime = false
        };

        handler.ValidateToken(tokenResult.AccessToken, validationParams, out var validatedToken);
        var jwt = (JwtSecurityToken)validatedToken;

        jwt.Subject.Should().Be(UserId);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == Email);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        var token = CreateService().GenerateRefreshToken();

        token.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(token);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void GenerateTokens_AccessTokenExpiry_MatchesSettings()
    {
        var before = DateTime.UtcNow;

        var tokenResult = CreateService().GenerateTokens("user-id", "user@test.com", ["User"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenResult.AccessToken);
        var expectedExpiry = before.AddMinutes(TestSettings.AccessTokenExpirationMinutes);
        jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }
}
