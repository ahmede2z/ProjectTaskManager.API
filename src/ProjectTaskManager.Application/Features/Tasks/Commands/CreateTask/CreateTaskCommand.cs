using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Common;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate) : IRequest<Result<TaskDto>>;
