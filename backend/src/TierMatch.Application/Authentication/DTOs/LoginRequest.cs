namespace TierMatch.Application.Authentication.DTOs;

public sealed record LoginRequest(
    string Email,
    string Password);