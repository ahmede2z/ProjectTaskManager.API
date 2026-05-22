using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Commands.UpdateProject;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Application.UnitTests.Features.Projects;

public sealed class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_OwnerUpdates_Succeeds()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Old Name", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new UpdateProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new UpdateProjectCommand(project.Id, "New Name", "Updated description"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_NonOwnerUpdates_ReturnsNotFound()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = "other-user-id" };
        var handler = new UpdateProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new UpdateProjectCommand(project.Id, "Hacked", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_NonExistentProject_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var userContext = new FakeUserContext { UserId = "user-id" };
        var handler = new UpdateProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new UpdateProjectCommand(Guid.NewGuid(), "Name", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
