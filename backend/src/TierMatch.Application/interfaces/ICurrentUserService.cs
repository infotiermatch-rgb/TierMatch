namespace TierMatch.Application.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? ShelterId { get; }

    bool IsInRole(string role);
}