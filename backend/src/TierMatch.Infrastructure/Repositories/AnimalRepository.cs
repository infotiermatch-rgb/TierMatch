using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public class AnimalRepository : IAnimalRepository
{
    private readonly AppDbContext _context;

    public AnimalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Animal?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Animals
            .Include(a => a.Shelter)
            .Include(a => a.Images)
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);
    }

    public async Task<List<Animal>> GetAllAsync(
        AnimalStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Animal> query = _context.Animals
            .Include(a => a.Shelter)
            .Include(a => a.Images);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Animal>> GetByShelterIdAsync(
        Guid shelterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Animals
            .Include(a => a.Shelter)
            .Include(a => a.Images)
            .Where(a => a.ShelterId == shelterId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Animal animal,
        CancellationToken cancellationToken = default)
    {
        await _context.Animals.AddAsync(
            animal,
            cancellationToken);
    }

    public void Update(Animal animal)
    {
        _context.Animals.Update(animal);
    }

    public void Delete(Animal animal)
    {
        _context.Animals.Remove(animal);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Animals
            .AnyAsync(
                a => a.Id == id,
                cancellationToken);
    }
}