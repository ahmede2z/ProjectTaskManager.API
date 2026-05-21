using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectTaskManager.API.Extensions;
using ProjectTaskManager.Application.Features.Projects.Commands.CreateProject;
using ProjectTaskManager.Application.Features.Projects.Commands.DeleteProject;
using ProjectTaskManager.Application.Features.Projects.Commands.UpdateProject;
using ProjectTaskManager.Application.Features.Projects.Queries.GetProjectById;
using ProjectTaskManager.Application.Features.Projects.Queries.GetProjects;

namespace ProjectTaskManager.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetProjectsQuery query, CancellationToken ct)
        => (await mediator.Send(query, ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => (await mediator.Send(new GetProjectByIdQuery(id), ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command, CancellationToken ct)
        => (await mediator.Send(command, ct)).ToCreatedAtAction(nameof(GetById), result => new { id = result.Id });

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new ProblemDetails { Title = "Route id and body id do not match.", Status = 400 });
        return (await mediator.Send(command, ct)).ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await mediator.Send(new DeleteProjectCommand(id), ct)).ToActionResult();
}
