namespace TierMatch.Application.Authentication.DTOs;

public sealed record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);