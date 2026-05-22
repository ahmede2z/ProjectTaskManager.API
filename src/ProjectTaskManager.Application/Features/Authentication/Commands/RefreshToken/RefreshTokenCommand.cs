using MediatR;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Common.Models.Authentication;

namespace ProjectTaskManager.Application.Features.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<TokenResult>>;
