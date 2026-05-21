using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Common;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;

public sealed class UpdateTaskStatusCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<UpdateTaskStatusCommand, Result<TaskDto>>
{
    public async Task<Result<TaskDto>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<TaskDto>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var task = await context.Tasks
            .Where(t => t.Id == request.TaskId
                     && t.ProjectId == request.ProjectId
                     && (userContext.IsAdmin || t.Project!.OwnerId == userContext.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (task is null)
            return Result<TaskDto>.Failure(new Error("Tasks.NotFound", "Task not found.", ErrorType.NotFound));

        task.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        return Result<TaskDto>.Success(new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.CreatedAt,
            task.ProjectId));
    }
}
