using System.Security.Cryptography;
using System.Text;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Infrastructure.Authentication;

public sealed class RefreshTokenService
    : IRefreshTokenService
{
    private static readonly TimeSpan Lifetime =
        TimeSpan.FromDays(30);

    public RefreshTokenResult Create(
        Guid userId,
        string? ipAddress,
        string? userAgent)
    {
        var plainTextToken = GenerateToken();

        var tokenHash = ComputeHash(
            plainTextToken);

        var refreshToken =
            new RefreshToken(
                userId,
                tokenHash,
                DateTime.UtcNow.Add(Lifetime),
                ipAddress,
                userAgent);

        return new RefreshTokenResult(
            plainTextToken,
            refreshToken);
    }

    public string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[64];

        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes);
    }

    public string ComputeHash(
        string token)
    {
        var hash =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash);
    }

    public bool Verify(
        string token,
        string hash)
    {
        var computedHash =
            ComputeHash(token);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computedHash),
            Convert.FromHexString(hash));
    }
}