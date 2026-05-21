using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<DeleteTaskCommand, Result>
{
    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var task = await context.Tasks
            .Where(t => t.Id == request.TaskId
                     && t.ProjectId == request.ProjectId
                     && (userContext.IsAdmin || t.Project!.OwnerId == userContext.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (task is null)
            return Result.Failure(new Error("Tasks.NotFound", "Task not found.", ErrorType.NotFound));

        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
