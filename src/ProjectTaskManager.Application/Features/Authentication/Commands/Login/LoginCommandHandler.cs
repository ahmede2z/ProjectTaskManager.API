using MediatR;
using Microsoft.Extensions.Logging;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<TokenResult>>
{
    public async Task<Result<TokenResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authResult = await identityService.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (authResult.IsFailure)
            return Result<TokenResult>.Failure(authResult.Error!);

        var user = authResult.Value!;
        var tokens = jwtTokenService.GenerateTokens(user.Id, user.Email, user.Roles);

        var storeResult = await identityService.SetRefreshTokenAsync(
            user.Id, tokens.RefreshToken, tokens.RefreshTokenExpiresAt, cancellationToken);
        if (storeResult.IsFailure)
            logger.LogWarning("Failed to store refresh token for user {UserId}: {Error}", user.Id, storeResult.Error!.Message);

        return Result<TokenResult>.Success(tokens);
    }
}
