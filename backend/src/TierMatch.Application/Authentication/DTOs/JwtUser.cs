namespace TierMatch.Application.Authentication.DTOs;

public sealed record JwtUser(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);