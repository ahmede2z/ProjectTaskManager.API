using FluentAssertions;
using ProjectTaskManager.Application.Features.Projects.Queries.GetProjects;
using ProjectTaskManager.Application.UnitTests.Common;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Application.UnitTests.Features.Projects;

public sealed class GetProjectsQueryHandlerTests
{
    [Fact]
    public async Task Handle_NormalUser_ReturnsOnlyOwnedProjects()
    {
        const string OwnerId = "owner-user-id";
        const string OtherId = "other-user-id";
        await using var context = TestDbContextFactory.Create();
        context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "My Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow });
        context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "Other Project", OwnerId = OtherId, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new GetProjectsQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value!.Items.Should().AllSatisfy(p => p.OwnerId.Should().Be(OwnerId));
    }

    [Fact]
    public async Task Handle_Admin_ReturnsAllProjects()
    {
        const string OwnerId = "owner-user-id";
        const string OtherId = "other-user-id";
        await using var context = TestDbContextFactory.Create();
        context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "Project 1", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow });
        context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "Project 2", OwnerId = OtherId, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = "admin-id", IsAdmin = true };
        var handler = new GetProjectsQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSearch_FiltersByNameSubstring()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "Alpha Project", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow });
        context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = "Beta Initiative", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new GetProjectsQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectsQuery { Search = "Alpha" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(1);
        result.Value!.Items[0].Name.Should().Be("Alpha Project");
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        const string OwnerId = "owner-user-id";
        await using var context = TestDbContextFactory.Create();
        for (var i = 1; i <= 5; i++)
            context.Projects.Add(new Project { Id = Guid.NewGuid(), Name = $"Project {i}", OwnerId = OwnerId, CreatedAt = DateTime.UtcNow.AddMinutes(-i) });
        await context.SaveChangesAsync(CancellationToken.None);

        var userContext = new FakeUserContext { UserId = OwnerId };
        var handler = new GetProjectsQueryHandler(context, userContext);

        var result = await handler.Handle(new GetProjectsQuery { PageNumber = 2, PageSize = 2 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PageNumber.Should().Be(2);
        result.Value!.Items.Count.Should().Be(2);
        result.Value!.TotalCount.Should().Be(5);
        result.Value!.HasPrevious.Should().BeTrue();
        result.Value!.HasNext.Should().BeTrue();
    }
}
