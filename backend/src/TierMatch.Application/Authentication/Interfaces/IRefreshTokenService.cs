using TierMatch.Application.Authentication.DTOs;

namespace TierMatch.Application.Authentication.Interfaces;

public interface IRefreshTokenService
{
    RefreshTokenResult Create(
        Guid userId,
        string? ipAddress,
        string? userAgent);

    string GenerateToken();

    string ComputeHash(
        string token);

    bool Verify(
        string token,
        string hash);
}