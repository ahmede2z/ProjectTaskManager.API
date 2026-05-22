using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Common;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.Features.Tasks.Queries.GetTasksByProject;

public sealed record GetTasksByProjectQuery : PagedRequest, IRequest<Result<PagedResult<TaskDto>>>
{
    public Guid ProjectId { get; init; }
    public TaskItemStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public DateTime? DueBefore { get; init; }
    public DateTime? DueAfter { get; init; }
}
