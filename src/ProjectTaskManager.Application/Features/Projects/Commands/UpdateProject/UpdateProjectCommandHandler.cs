using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;

namespace ProjectTaskManager.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<ProjectDto>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
            return Result<ProjectDto>.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        if (!userContext.IsAdmin && project.OwnerId != userContext.UserId)
            return Result<ProjectDto>.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        project.Name = request.Name;
        project.Description = request.Description;

        await context.SaveChangesAsync(cancellationToken);

        var taskCount = await context.Tasks
            .CountAsync(t => t.ProjectId == project.Id, cancellationToken);

        return Result<ProjectDto>.Success(new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt,
            project.OwnerId,
            taskCount));
    }
}
