namespace TierMatch.Application.Authentication.DTOs;

public sealed record UpdateCurrentUserProfileRequest(
    string FirstName,
    string LastName);