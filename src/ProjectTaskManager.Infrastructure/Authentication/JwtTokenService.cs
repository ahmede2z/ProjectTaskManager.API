using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public TokenResult GenerateTokens(string userId, string email, IEnumerable<string> roles)
    {
        if (string.IsNullOrWhiteSpace(_settings.Secret) || _settings.Secret.Length < 32)
            throw new InvalidOperationException("JWT secret must be at least 32 characters.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var accessExpiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: accessExpiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        return new TokenResult(accessToken, refreshToken, accessExpiresAt, refreshExpiresAt);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
