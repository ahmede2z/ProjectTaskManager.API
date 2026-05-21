using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;

namespace ProjectTaskManager.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(string Name, string? Description) : IRequest<Result<ProjectDto>>;
