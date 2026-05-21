using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Commands.CreateTask;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.UnitTests.Features.Tasks;

public sealed class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_OwnerOfProject_CreatesTask()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new CreateTaskCommandHandler(context, userContext);

        var result = await handler.Handle(
            new CreateTaskCommand(project.Id, "My Task", null, TaskPriority.Medium, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ProjectId.Should().Be(project.Id);
        result.Value!.Title.Should().Be("My Task");
        result.Value!.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task Handle_NonOwnerOfProject_ReturnsNotFound()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = "other-user-id" };
        var handler = new CreateTaskCommandHandler(context, userContext);

        var result = await handler.Handle(
            new CreateTaskCommand(project.Id, "My Task", null, TaskPriority.Medium, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
