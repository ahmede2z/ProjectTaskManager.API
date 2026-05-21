using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Commands.DeleteProject;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Application.UnitTests.Features.Projects;

public sealed class DeleteProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_OwnerDeletes_Succeeds()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId, Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new DeleteProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new DeleteProjectCommand(projectId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var exists = await context.Projects.AnyAsync(p => p.Id == projectId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonOwnerDeletes_ReturnsNotFound()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = "other-user-id" };
        var handler = new DeleteProjectCommandHandler(context, userContext);

        var result = await handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
