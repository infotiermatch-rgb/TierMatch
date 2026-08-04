namespace TierMatch.Application.Authentication.DTOs;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);