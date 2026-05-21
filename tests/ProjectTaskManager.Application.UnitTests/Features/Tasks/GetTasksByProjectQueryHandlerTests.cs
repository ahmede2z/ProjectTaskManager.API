using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Queries.GetTasksByProject;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.UnitTests.Features.Tasks;

public sealed class GetTasksByProjectQueryHandlerTests
{
    [Fact]
    public async Task Handle_OwnerOfProject_ReturnsTasks()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        context.Tasks.Add(new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", ProjectId = project.Id, Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow });
        context.Tasks.Add(new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", ProjectId = project.Id, Status = TaskItemStatus.Done, Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new GetTasksByProjectQueryHandler(context, userContext);

        var result = await handler.Handle(new GetTasksByProjectQuery { ProjectId = project.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
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
        var handler = new GetTasksByProjectQueryHandler(context, userContext);

        var result = await handler.Handle(new GetTasksByProjectQuery { ProjectId = project.Id }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingStatus()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        context.Tasks.Add(new TaskItem { Id = Guid.NewGuid(), Title = "Todo Task", ProjectId = project.Id, Status = TaskItemStatus.Todo, Priority = TaskPriority.Medium, CreatedAt = DateTime.UtcNow });
        context.Tasks.Add(new TaskItem { Id = Guid.NewGuid(), Title = "Done Task", ProjectId = project.Id, Status = TaskItemStatus.Done, Priority = TaskPriority.Low, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new GetTasksByProjectQueryHandler(context, userContext);

        var result = await handler.Handle(
            new GetTasksByProjectQuery { ProjectId = project.Id, Status = TaskItemStatus.Todo },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value!.Items.Should().AllSatisfy(t => t.Status.Should().Be(TaskItemStatus.Todo));
    }
}
