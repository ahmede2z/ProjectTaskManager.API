using FluentAssertions;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Queries.GetProjectById;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Application.UnitTests.Features.Projects;

public sealed class GetProjectByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_OwnerRequestsTheirProject_ReturnsProject()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new GetProjectByIdQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(project.Id);
        result.Value!.OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task Handle_NonOwnerRequestsSomeoneElsesProject_ReturnsNotFound()
    {
        const string OwnerId = "owner-user-id";
        const string OtherId = "other-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OtherId };
        var handler = new GetProjectByIdQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        // Must be NotFound, not Forbidden — no information leak about project existence
        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_AdminRequestsAnyProject_ReturnsProject()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        var project = new Project { Id = Guid.NewGuid(), Name = "Test Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow };
        context.Projects.Add(project);
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = "admin-user-id", IsAdmin = true };
        var handler = new GetProjectByIdQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(project.Id);
    }

    [Fact]
    public async Task Handle_ProjectDoesNotExist_ReturnsNotFound()
    {
        await using var context = TestDbContextFactory.Create();
        var userContext = new FakeUserContext { UserId = "user-id" };
        var handler = new GetProjectByIdQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
