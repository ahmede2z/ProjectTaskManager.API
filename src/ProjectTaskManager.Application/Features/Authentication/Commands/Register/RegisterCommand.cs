using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.Features.Authentication.Commands.Register;

public sealed record RegisterCommand(string Email, string Password) : IRequest<Result<TokenResult>>;
