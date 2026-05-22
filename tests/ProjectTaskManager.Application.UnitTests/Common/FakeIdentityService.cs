using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.UnitTests.Common;

public sealed class FakeIdentityService : IIdentityService
{
    public Func<string, string, Result<string>>? OnCreateUser { get; set; }
    public Func<string, string, Result<AuthUser>>? OnAuthenticate { get; set; }
    public Func<string, Result<AuthUser>>? OnFindByRefreshToken { get; set; }
    public Func<string, string, DateTime, Result>? OnSetRefreshToken { get; set; }
    public Func<string, string, Result>? OnAddToRole { get; set; }

    public Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken ct)
        => Task.FromResult(OnCreateUser?.Invoke(email, password)
            ?? throw new InvalidOperationException("OnCreateUser not configured"));

    public Task<Result<AuthUser>> AuthenticateAsync(string email, string password, CancellationToken ct)
        => Task.FromResult(OnAuthenticate?.Invoke(email, password)
            ?? throw new InvalidOperationException("OnAuthenticate not configured"));

    public Task<Result<AuthUser>> FindByRefreshTokenAsync(string refreshToken, CancellationToken ct)
        => Task.FromResult(OnFindByRefreshToken?.Invoke(refreshToken)
            ?? throw new InvalidOperationException("OnFindByRefreshToken not configured"));

    public Task<Result> SetRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, CancellationToken ct)
        => Task.FromResult(OnSetRefreshToken?.Invoke(userId, refreshToken, expiresAt)
            ?? throw new InvalidOperationException("OnSetRefreshToken not configured"));

    public Task<Result> AddToRoleAsync(string userId, string roleName, CancellationToken ct)
        => Task.FromResult(OnAddToRole?.Invoke(userId, roleName)
            ?? throw new InvalidOperationException("OnAddToRole not configured"));
}
