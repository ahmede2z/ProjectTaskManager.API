using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Common;
using ProjectTaskManager.Domain.Entities;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<TaskDto>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var projectExists = await context.Projects
            .AnyAsync(p => p.Id == request.ProjectId
                        && (userContext.IsAdmin || p.OwnerId == userContext.UserId), cancellationToken);

        if (!projectExists)
            return Result<TaskDto>.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Status = TaskItemStatus.Todo,
            CreatedAt = DateTime.UtcNow,
            ProjectId = request.ProjectId
        };

        context.Tasks.Add(task);
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
