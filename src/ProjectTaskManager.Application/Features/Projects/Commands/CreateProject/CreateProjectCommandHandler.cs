using MediatR;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<ProjectDto>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userContext.UserId
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt,
            project.OwnerId,
            TaskCount: 0));
    }
}
