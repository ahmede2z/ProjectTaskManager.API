using FluentValidation;

namespace ProjectTaskManager.Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MinimumLength(20);
    }
}
