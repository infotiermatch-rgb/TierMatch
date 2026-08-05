namespace TierMatch.Application.Authentication.DTOs;

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);