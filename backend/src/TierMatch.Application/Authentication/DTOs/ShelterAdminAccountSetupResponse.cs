namespace TierMatch.Application.Authentication.DTOs;

public sealed record ShelterAdminAccountSetupResponse(
    Guid UserId,
    string SetupToken);