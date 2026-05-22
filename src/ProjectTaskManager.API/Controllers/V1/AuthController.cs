using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectTaskManager.API.Extensions;
using ProjectTaskManager.Application.Features.Authentication.Commands.Login;
using ProjectTaskManager.Application.Features.Authentication.Commands.RefreshToken;
using ProjectTaskManager.Application.Features.Authentication.Commands.Register;

namespace ProjectTaskManager.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        => (await mediator.Send(command, cancellationToken)).ToActionResult();

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        => (await mediator.Send(command, cancellationToken)).ToActionResult();

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        => (await mediator.Send(command, cancellationToken)).ToActionResult();
}
