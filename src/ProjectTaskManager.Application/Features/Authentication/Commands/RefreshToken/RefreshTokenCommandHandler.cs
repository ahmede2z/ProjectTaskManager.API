using MediatR;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IIdentityService identityService,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, Result<TokenResult>>
{
    public async Task<Result<TokenResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var findResult = await identityService.FindByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (findResult.IsFailure)
            return Result<TokenResult>.Failure(findResult.Error!);

        var user = findResult.Value!;
        var tokens = jwtTokenService.GenerateTokens(user.Id, user.Email, user.Roles);

        var storeResult = await identityService.SetRefreshTokenAsync(
            user.Id, tokens.RefreshToken, tokens.RefreshTokenExpiresAt, cancellationToken);
        if (storeResult.IsFailure)
            return Result<TokenResult>.Failure(storeResult.Error!);

        return Result<TokenResult>.Success(tokens);
    }
}
