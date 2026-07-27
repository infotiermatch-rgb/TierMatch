using TierMatch.Domain.Entities;
using TierMatch.Application.Animals.Models;

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

}