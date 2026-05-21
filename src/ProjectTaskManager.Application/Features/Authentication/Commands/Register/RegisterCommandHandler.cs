using MediatR;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;
using ProjectTaskManager.Domain.Constants;

namespace ProjectTaskManager.Application.Features.Authentication.Commands.Register;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService) : IRequestHandler<RegisterCommand, Result<TokenResult>>
{
    public async Task<Result<TokenResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var createResult = await identityService.CreateUserAsync(request.Email, request.Password, cancellationToken);
        if (createResult.IsFailure)
            return Result<TokenResult>.Failure(createResult.Error!);

        var userId = createResult.Value!;

        var roleResult = await identityService.AddToRoleAsync(userId, Roles.User, cancellationToken);
        if (roleResult.IsFailure)
            return Result<TokenResult>.Failure(roleResult.Error!);

        var tokens = jwtTokenService.GenerateTokens(userId, request.Email, [Roles.User]);

        var storeResult = await identityService.SetRefreshTokenAsync(
            userId, tokens.RefreshToken, tokens.RefreshTokenExpiresAt, cancellationToken);
        if (storeResult.IsFailure)
            return Result<TokenResult>.Failure(storeResult.Error!);

        return Result<TokenResult>.Success(tokens);
    }
}
