using MediatR;
using ProjectTaskManager.Application.Common.Models;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(Guid ProjectId, Guid TaskId) : IRequest<Result>;
