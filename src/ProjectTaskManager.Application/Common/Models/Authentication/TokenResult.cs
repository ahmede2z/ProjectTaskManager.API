namespace ProjectTaskManager.Application.Common.Models.Authentication;

public sealed record TokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
