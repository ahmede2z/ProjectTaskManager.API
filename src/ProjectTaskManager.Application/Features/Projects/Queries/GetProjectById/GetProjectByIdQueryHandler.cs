using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectTaskManager.Application.Common.Interfaces;
using ProjectTaskManager.Application.Common.Models;
using ProjectTaskManager.Application.Features.Projects.Common;

namespace ProjectTaskManager.Application.Features.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (userContext.UserId is null)
            return Result<ProjectDto>.Failure(new Error("Auth.Unauthorized", "User is not authenticated.", ErrorType.Unauthorized));

        var query = context.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.Id);

        if (!userContext.IsAdmin)
            query = query.Where(p => p.OwnerId == userContext.UserId);

        var dto = await query
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.CreatedAt,
                p.OwnerId,
                p.Tasks.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<ProjectDto>.Failure(new Error("Projects.NotFound", "Project not found.", ErrorType.NotFound));

        return Result<ProjectDto>.Success(dto);
    }
}
