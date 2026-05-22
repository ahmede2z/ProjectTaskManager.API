using FluentValidation;

namespace ProjectTaskManager.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);
    }
}
