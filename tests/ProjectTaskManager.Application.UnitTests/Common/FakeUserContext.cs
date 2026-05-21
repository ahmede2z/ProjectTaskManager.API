using ProjectTaskManager.Application.Common.Interfaces;

namespace ProjectTaskManager.Application.UnitTests.Common;

public sealed class FakeUserContext : IUserContext
{
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsAuthenticated => UserId is not null;
}
