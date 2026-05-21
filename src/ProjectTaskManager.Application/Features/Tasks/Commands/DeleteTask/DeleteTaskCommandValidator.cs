using FluentValidation;

namespace ProjectTaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.TaskId)
            .NotEmpty();
    }
}
