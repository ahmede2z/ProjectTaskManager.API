using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public TaskItemStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public required Guid ProjectId { get; init; }
    public Project? Project { get; init; }
}
