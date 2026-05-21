using MediatR;
using ProjectTaskManager.Application.Common.Models;

namespace ProjectTaskManager.Application.Features.Projects.Commands.DeleteProject;

public sealed record DeleteProjectCommand(Guid Id) : IRequest<Result>;
