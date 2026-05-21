using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Extensions;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Tasks.Common;
using ProjectTaskManager.Domain.Entities;
using ProjectTaskManager.Domain.Enums;

namespace ProjectTaskManager.Application.Features.Tasks.Queries.GetTasksByProject;

public sealed class GetTasksByProjectQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<GetTasksByProjectQuery, Result<PagedResult<TaskDto>>>
{
    public async Task<Result<PagedResult<TaskDto>>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<PagedResult<TaskDto>>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var projectExists = await context.Projects
            .AnyAsync(p => p.Id == request.ProjectId
                        && (userContext.IsAdmin || p.OwnerId == userContext.UserId), cancellationToken);

        if (!projectExists)
            return Result<PagedResult<TaskDto>>.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        var query = context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == request.ProjectId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => EF.Functions.Like(t.Title, $"%{request.Search}%"));

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.Priority.HasValue)
            query = query.Where(t => t.Priority == request.Priority.Value);

        if (request.DueBefore.HasValue)
            query = query.Where(t => t.DueDate < request.DueBefore.Value);

        if (request.DueAfter.HasValue)
            query = query.Where(t => t.DueDate > request.DueAfter.Value);

        query = ApplySorting(query, request.SortBy, request.SortDirection);

        var projection = query.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.CreatedAt,
            t.ProjectId));

        var result = await projection.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result<PagedResult<TaskDto>>.Success(result);
    }

    private static IQueryable<TaskItem> ApplySorting(
        IQueryable<TaskItem> query,
        string? sortBy,
        string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "priority" => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "duedate" => descending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "createdat" => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };
    }
}
