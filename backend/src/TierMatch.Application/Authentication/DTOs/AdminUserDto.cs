namespace TierMatch.Application.Authentication.DTOs;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles,
    Guid? ShelterId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);