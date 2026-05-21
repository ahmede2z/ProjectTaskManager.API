using FluentValidation;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;

public sealed class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
