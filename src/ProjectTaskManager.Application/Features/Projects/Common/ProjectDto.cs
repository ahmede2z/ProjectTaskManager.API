namespace ProjectTaskManager.Application.Features.Projects.Common;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    string OwnerId,
    int TaskCount);
