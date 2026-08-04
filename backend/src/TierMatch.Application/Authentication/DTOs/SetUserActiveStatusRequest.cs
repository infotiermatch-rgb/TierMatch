namespace TierMatch.Application.Authentication.DTOs;

public sealed record SetUserActiveStatusRequest(
    bool IsActive);