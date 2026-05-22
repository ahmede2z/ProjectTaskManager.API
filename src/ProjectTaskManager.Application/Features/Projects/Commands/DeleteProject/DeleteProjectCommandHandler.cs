using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;

namespace ProjectTaskManager.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<DeleteProjectCommand, Result>
{
    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
            return Result.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        if (!userContext.IsAdmin && project.OwnerId != userContext.UserId)
            return Result.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        context.Projects.Remove(project);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
