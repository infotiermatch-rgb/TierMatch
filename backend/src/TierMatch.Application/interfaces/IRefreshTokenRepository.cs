using TierMatch.Domain.Entities;

namespace TierMatch.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}