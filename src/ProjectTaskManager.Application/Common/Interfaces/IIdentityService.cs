using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthUser>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthUser>> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<Result> SetRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken);
    Task<Result> AddToRoleAsync(string userId, string roleName, CancellationToken cancellationToken);
}
