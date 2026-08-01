using TierMatch.Domain.Entities;

namespace TierMatch.Application.Interfaces;

public interface IAnimalImageRepository
{
    Task<List<AnimalImage>> GetByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task<AnimalImage?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AnimalImage?> GetPrimaryAsync(
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSortOrderAsync(
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AnimalImage image,
        CancellationToken cancellationToken = default);

    void Update(AnimalImage image);

    void Delete(AnimalImage image);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

Task<List<AnimalImage>> GetAllByAnimalIdAsync(
    Guid animalId,
    CancellationToken cancellationToken = default);
}