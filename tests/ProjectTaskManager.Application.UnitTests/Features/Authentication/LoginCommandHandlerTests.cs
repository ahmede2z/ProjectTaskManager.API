using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;
using ProjectTaskManager.Application.Features.Authentication.Commands.Login;
using ProjectTaskManager.Application.UnitTests.Common;

namespace ProjectTaskManager.Application.UnitTests.Features.Authentication;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithTokens()
    {
        const string UserId = "user-1";
        const string Email = "user@test.com";

        var identityService = new FakeIdentityService
        {
            OnAuthenticate = (_, _) => Result<AuthUser>.Success(new AuthUser(UserId, Email, ["User"])),
            OnSetRefreshToken = (_, _, _) => Result.Success()
        };
        var handler = new LoginCommandHandler(identityService, new FakeJwtTokenService(), NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(new LoginCommand(Email, "Password1!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
        result.Value!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ReturnsUnauthorized()
    {
        var identityService = new FakeIdentityService
        {
            OnAuthenticate = (_, _) => Result<AuthUser>.Failure(
                new Error("Auth.InvalidCredentials", "Email or password is incorrect.", ErrorType.Unauthorized))
        };
        var handler = new LoginCommandHandler(identityService, new FakeJwtTokenService(), NullLogger<LoginCommandHandler>.Instance);

        var result = await handler.Handle(new LoginCommand("bad@test.com", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }
}
