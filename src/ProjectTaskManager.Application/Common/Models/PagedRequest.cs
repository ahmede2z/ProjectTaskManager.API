namespace ProjectTaskManager.Application.Common.Models;

public abstract record PagedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "desc";
    public string? Search { get; init; }
}
