using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Extensions;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;
using ProjectTaskManager.Domain.Entities;

namespace ProjectTaskManager.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<GetProjectsQuery, Result<PagedResult<ProjectDto>>>
{
    public async Task<Result<PagedResult<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<PagedResult<ProjectDto>>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var query = context.Projects.AsNoTracking();

        if (!userContext.IsAdmin)
            query = query.Where(p => p.OwnerId == userContext.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{request.Search}%"));

        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var projection = query.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.Description,
            p.CreatedAt,
            p.OwnerId,
            p.Tasks.Count));

        var result = await projection.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result<PagedResult<ProjectDto>>.Success(result);
    }

    private static IQueryable<Project> ApplySorting(
        IQueryable<Project> query,
        string? sortBy,
        string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
