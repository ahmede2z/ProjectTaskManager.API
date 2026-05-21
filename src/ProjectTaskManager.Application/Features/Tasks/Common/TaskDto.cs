using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.Features.Tasks.Common;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    Guid ProjectId);
