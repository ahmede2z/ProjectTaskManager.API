using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Commands.DeleteTask;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.UnitTests.Features.Tasks;

public sealed class DeleteTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_OwnerOfParentProject_DeletesTask()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        var taskId = Guid.NewGuid();
        var task = new TaskItem { Id = taskId, Title = "Task", ProjectId = project.Id, Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        context.Tasks.Add(task);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new DeleteTaskCommandHandler(context, userContext);

        var result = await handler.Handle(new DeleteTaskCommand(project.Id, taskId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var exists = await context.Tasks.AnyAsync(t => t.Id == taskId);
        exists.Should().BeFalse();
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
        var handler = new DeleteTaskCommandHandler(context, userContext);

        var result = await handler.Handle(new DeleteTaskCommand(project.Id, task.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
