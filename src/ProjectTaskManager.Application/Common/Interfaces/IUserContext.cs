namespace ProjectTaskManager.Application.Common.Interfaces;

public interface IUserContext
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
