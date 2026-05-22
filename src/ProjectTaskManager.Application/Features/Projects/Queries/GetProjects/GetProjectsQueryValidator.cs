using FluentValidation;

namespace ProjectTaskManager.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
{
    private static readonly string[] AllowedSortBy = ["name", "createdat"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetProjectsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortDirection)
            .Must(d => AllowedSortDirections.Contains(d.ToLowerInvariant()))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");

        RuleFor(x => x.SortBy)
            .Must(s => s is null || AllowedSortBy.Contains(s.ToLowerInvariant()))
            .WithMessage("SortBy must be 'name' or 'createdAt'.");
    }
}
