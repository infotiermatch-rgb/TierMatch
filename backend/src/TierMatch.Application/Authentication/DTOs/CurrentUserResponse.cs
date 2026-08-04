namespace TierMatch.Application.Authentication.DTOs;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles,
    Guid? ShelterId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);