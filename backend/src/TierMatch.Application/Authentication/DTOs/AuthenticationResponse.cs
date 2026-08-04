namespace TierMatch.Application.Authentication.DTOs;

public sealed class AuthenticationResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAt { get; init; }

    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public IReadOnlyList<string> Roles { get; init; }
        = [];
}