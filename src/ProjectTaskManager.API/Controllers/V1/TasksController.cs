using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectTaskManager.API.Extensions;
using ProjectTaskManager.Application.Features.Tasks.Commands.CreateTask;
using ProjectTaskManager.Application.Features.Tasks.Commands.DeleteTask;
using ProjectTaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;
using ProjectTaskManager.Application.Features.Tasks.Queries.GetTasksByProject;

namespace ProjectTaskManager.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:guid}/tasks")]
[Authorize]
public sealed class TasksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByProject(
        [FromRoute] Guid projectId,
        [FromQuery] GetTasksByProjectQuery query,
        CancellationToken ct)
    {
        var effective = query with { ProjectId = projectId };
        return (await mediator.Send(effective, ct)).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromBody] CreateTaskCommand command,
        CancellationToken ct)
    {
        var effective = command with { ProjectId = projectId };
        return (await mediator.Send(effective, ct))
            .ToCreatedAtAction(nameof(GetByProject), _ => new { projectId });
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        [FromBody] UpdateTaskStatusCommand command,
        CancellationToken ct)
    {
        var effective = command with { ProjectId = projectId, TaskId = taskId };
        return (await mediator.Send(effective, ct)).ToActionResult();
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromRoute] Guid taskId,
        CancellationToken ct)
        => (await mediator.Send(new DeleteTaskCommand(projectId, taskId), ct)).ToActionResult();
}
