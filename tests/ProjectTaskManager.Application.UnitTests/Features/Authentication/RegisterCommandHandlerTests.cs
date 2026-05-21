using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Authentication.Commands.Register;
using ProjectTaskManager.Application.UnitTests.Common;

namespace ProjectTaskManager.Application.UnitTests.Features.Authentication;

public sealed class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_NewEmail_ReturnsSuccessWithTokens()
    {
        const string UserId = "new-user-id";
        const string Email = "new@test.com";

        var identityService = new FakeIdentityService
        {
            OnCreateUser = (_, _) => Result<string>.Success(UserId),
            OnAddToRole = (_, _) => Result.Success(),
            OnSetRefreshToken = (_, _, _) => Result.Success()
        };
        var handler = new RegisterCommandHandler(identityService, new FakeJwtTokenService());

        var result = await handler.Handle(new RegisterCommand(Email, "Password1!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        var identityService = new FakeIdentityService
        {
            OnCreateUser = (_, _) => Result<string>.Failure(
                new Error("Auth.EmailInUse", "An account with this email already exists.", ErrorType.Conflict))
        };
        var handler = new RegisterCommandHandler(identityService, new FakeJwtTokenService());

        var result = await handler.Handle(new RegisterCommand("existing@test.com", "Password1!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }
}
