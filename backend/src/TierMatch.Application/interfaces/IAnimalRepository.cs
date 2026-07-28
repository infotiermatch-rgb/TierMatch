using TierMatch.Domain.Entities;
using TierMatch.Application.Animals.Models;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Interfaces;

public interface IAnimalRepository
{
    Task<Animal?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Animal>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Animal animal,
        CancellationToken cancellationToken = default);

    void Update(Animal animal);

    void Delete(Animal animal);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

        Task<List<Animal>> GetByShelterIdAsync(
    Guid shelterId,
    CancellationToken cancellationToken = default);

    Task<List<Animal>> GetAllAsync(
    AnimalStatus? status,
    CancellationToken cancellationToken = default);

}