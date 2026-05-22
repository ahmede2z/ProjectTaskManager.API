using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Common;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;

public sealed record UpdateTaskStatusCommand(
    Guid ProjectId,
    Guid TaskId,
    TaskItemStatus Status) : IRequest<Result<TaskDto>>;
