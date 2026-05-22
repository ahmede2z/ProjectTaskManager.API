using FluentValidation;

namespace ProjectTaskManager.Application.Features.Tasks.Queries.GetTasksByProject;

public sealed class GetTasksByProjectQueryValidator : AbstractValidator<GetTasksByProjectQuery>
{
    private static readonly string[] AllowedSortBy = ["title", "priority", "duedate", "createdat"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetTasksByProjectQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortDirection)
            .Must(d => AllowedSortDirections.Contains(d.ToLowerInvariant()))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");

        RuleFor(x => x.SortBy)
            .Must(s => s is null || AllowedSortBy.Contains(s.ToLowerInvariant()))
            .WithMessage("SortBy must be one of: 'title', 'priority', 'dueDate', 'createdAt'.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.DueBefore)
            .GreaterThan(x => x.DueAfter)
            .WithMessage("DueBefore must be greater than DueAfter.")
            .When(x => x.DueBefore.HasValue && x.DueAfter.HasValue);
    }
}
