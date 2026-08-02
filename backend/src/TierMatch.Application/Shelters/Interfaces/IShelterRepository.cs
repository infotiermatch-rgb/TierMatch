using TierMatch.Domain.Entities;

namespace TierMatch.Application.Interfaces;

public interface IShelterRepository
{
    Task<Shelter?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Shelter>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Shelter shelter,
        CancellationToken cancellationToken = default);

    void Update(Shelter shelter);

    void Delete(Shelter shelter);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}