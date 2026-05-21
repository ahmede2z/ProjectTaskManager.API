namespace ProjectTaskManager.Application.Common.Models.Authentication;

public sealed record AuthUser(string Id, string Email, IReadOnlyList<string> Roles);
