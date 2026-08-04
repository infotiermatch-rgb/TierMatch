namespace TierMatch.Application.Authentication.DTOs;

public sealed record LogoutRequest(
    string RefreshToken);