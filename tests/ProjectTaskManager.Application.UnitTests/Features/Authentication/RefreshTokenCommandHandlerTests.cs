using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;
using ProjectTaskManager.Application.Features.Authentication.Commands.RefreshToken;
using ProjectTaskManager.Application.UnitTests.Common;

namespace ProjectTaskManager.Application.UnitTests.Features.Authentication;

public sealed class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        var identityService = new FakeIdentityService
        {
            OnFindByRefreshToken = _ => Result<AuthUser>.Success(
                new AuthUser("user-1", "user@test.com", ["User"])),
            OnSetRefreshToken = (_, _, _) => Result.Success()
        };
        var handler = new RefreshTokenCommandHandler(identityService, new FakeJwtTokenService());

        var result = await handler.Handle(new RefreshTokenCommand("valid-refresh-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ReturnsUnauthorized()
    {
        var identityService = new FakeIdentityService
        {
            OnFindByRefreshToken = _ => Result<AuthUser>.Failure(
                new Error("Auth.InvalidRefreshToken", "The refresh token is invalid.", ErrorType.Unauthorized))
        };
        var handler = new RefreshTokenCommandHandler(identityService, new FakeJwtTokenService());

        var result = await handler.Handle(new RefreshTokenCommand("bad-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }
}
