using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Interfaces;

public interface IAdoptionRequestRepository
{
    Task<AdoptionRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdoptionRequest?> GetByIdAndShelterIdAsync(
        Guid id,
        Guid shelterId,
        CancellationToken cancellationToken = default);

    Task<List<AdoptionRequest>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<AdoptionRequest>> GetByShelterIdAsync(
        Guid shelterId,
        CancellationToken cancellationToken = default);

    Task<List<AdoptionRequest>> GetByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task<List<AdoptionRequest>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<AdoptionRequest>> GetByStatusAsync(
        AdoptionRequestStatus status,
        CancellationToken cancellationToken = default);

    Task<List<AdoptionRequest>> GetPendingByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingRequestAsync(
        Guid userId,
        Guid animalId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AdoptionRequest request,
        CancellationToken cancellationToken = default);

    void Update(AdoptionRequest request);

    void Delete(AdoptionRequest request);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}