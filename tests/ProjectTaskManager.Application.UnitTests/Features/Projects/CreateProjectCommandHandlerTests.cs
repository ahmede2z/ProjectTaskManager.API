using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Commands.CreateProject;
using ProjectTaskManager.Application.UnitTests.Common;

namespace ProjectTaskManager.Application.UnitTests.Features.Projects;

public sealed class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_AuthenticatedUser_CreatesProjectWithCorrectOwner()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new CreateProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new CreateProjectCommand("My Project", "A description"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OwnerId.Should().Be(OwnerId);
        result.Value!.Name.Should().Be("My Project");
    }

    [Fact]
    public async Task Handle_NullUserId_ReturnsUnauthorized()
    {
        await using var context = TestDbContextFactory.Create();
        var userContext = new FakeUserContext { UserId = null };
        var handler = new CreateProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new CreateProjectCommand("My Project", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }
}
