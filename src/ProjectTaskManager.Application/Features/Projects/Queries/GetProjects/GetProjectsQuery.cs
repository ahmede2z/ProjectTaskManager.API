using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;

namespace ProjectTaskManager.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery : PagedRequest, IRequest<Result<PagedResult<ProjectDto>>>;
