using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.UnitTests.Features.Tasks;

public sealed class UpdateTaskStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_OwnerOfParentProject_UpdatesStatus()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task", ProjectId = project.Id, Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new UpdateTaskStatusCommandHandler(context, userContext);

        var result = await handler.Handle(
            new UpdateTaskStatusCommand(project.Id, task.Id, TaskItemStatus.InProgress),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public async Task Handle_NonOwnerOfParentProject_ReturnsNotFound()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "Task", ProjectId = project.Id, Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = "other-user-id" };
        var handler = new UpdateTaskStatusCommandHandler(context, userContext);

        var result = await handler.Handle(
            new UpdateTaskStatusCommand(project.Id, task.Id, TaskItemStatus.InProgress),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_NonExistentTask_ReturnsNotFound()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new UpdateTaskStatusCommandHandler(context, userContext);

        var result = await handler.Handle(
            new UpdateTaskStatusCommand(project.Id, Guid.NewGuid(), TaskItemStatus.InProgress),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
