using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;

namespace ProjectTaskManager.Application.Features.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(Guid Id, string Name, string? Description) : IRequest<Result<ProjectDto>>;
