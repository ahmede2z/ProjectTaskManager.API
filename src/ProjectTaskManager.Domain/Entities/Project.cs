namespace ProjectTaskManager.Domain.Entities;

public class Project
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public required string OwnerId { get; init; }
    public ICollection<TaskItem> Tasks { get; init; } = new List<TaskItem>();
}
