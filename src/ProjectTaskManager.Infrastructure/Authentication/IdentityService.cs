using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;
using ProjectTaskManager.Infrastructure.Identity;
using ProjectTaskManager.Infrastructure.Persistence;

namespace ProjectTaskManager.Infrastructure.Authentication;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<Result<string>> CreateUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Result<string>.Failure(new Error("Auth.EmailInUse", "An account with this email already exists.", ErrorType.Conflict));

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<string>.Failure(new Error("Auth.CreateFailed", message, ErrorType.Validation));
        }

        return Result<string>.Success(user.Id);
    }

    public async Task<Result<AuthUser>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        // Avoid username enumeration — same response whether email is unknown or password is wrong
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            return Result<AuthUser>.Failure(new Error("Auth.InvalidCredentials", "Email or password is incorrect.", ErrorType.Unauthorized));

        var roles = await userManager.GetRolesAsync(user);
        return Result<AuthUser>.Success(new AuthUser(user.Id, user.Email!, roles.ToList()));
    }

    public async Task<Result<AuthUser>> FindByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        if (user is null)
            return Result<AuthUser>.Failure(new Error("Auth.InvalidRefreshToken", "The refresh token is invalid.", ErrorType.Unauthorized));

        if (user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Result<AuthUser>.Failure(new Error("Auth.ExpiredRefreshToken", "The refresh token has expired.", ErrorType.Unauthorized));

        var roles = await userManager.GetRolesAsync(user);
        return Result<AuthUser>.Success(new AuthUser(user.Id, user.Email!, roles.ToList()));
    }

    public async Task<Result> SetRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(new Error("Auth.UserNotFound", "User not found.", ErrorType.NotFound));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = expiresAt;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to update refresh token for user {UserId}: {Message}", userId, message);
            return Result.Failure(new Error("Auth.UpdateFailed", "Failed to persist refresh token.", ErrorType.Unexpected));
        }

        return Result.Success();
    }

    public async Task<Result> AddToRoleAsync(string userId, string roleName, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(new Error("Auth.UserNotFound", "User not found.", ErrorType.NotFound));

        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to add role {Role} to user {UserId}: {Message}", roleName, userId, message);
            return Result.Failure(new Error("Auth.AddRoleFailed", "Failed to assign user role.", ErrorType.Unexpected));
        }

        return Result.Success();
    }
}
