using Microsoft.EntityFrameworkCore;

using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenRepository(
        AppDbContext context,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _refreshTokenService = refreshTokenService;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHash = _refreshTokenService.ComputeHash(token);

        return await _context.RefreshTokens
            .FirstOrDefaultAsync(
                refreshToken =>
                    refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>>
        GetActiveTokensByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Array.Empty<RefreshToken>();
        }

        var currentTime = DateTime.UtcNow;

        return await _context.RefreshTokens
            .Where(refreshToken =>
                refreshToken.UserId == userId &&
                refreshToken.RevokedAt == null &&
                refreshToken.ExpiresAt > currentTime)
            .OrderByDescending(refreshToken =>
                refreshToken.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}