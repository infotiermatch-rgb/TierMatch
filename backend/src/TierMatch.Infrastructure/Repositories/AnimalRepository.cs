using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Infrastructure.Data;
using TierMatch.Domain.Enums;

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
        .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
}
    public async Task<List<Animal>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Animals
            .Include(a => a.Shelter)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Animal animal,
        CancellationToken cancellationToken = default)
    {
        await _context.Animals.AddAsync(animal, cancellationToken);
    }

    public void Update(Animal animal)
    {
        _context.Animals.Update(animal);
    }

    public void Delete(Animal animal)
    {
        _context.Animals.Remove(animal);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Animal>> GetByShelterIdAsync(
    Guid shelterId,
    CancellationToken cancellationToken = default)
{
    return await _context.Animals
        .Include(a => a.Shelter)
        .Where(a => a.ShelterId == shelterId)
        .ToListAsync(cancellationToken);
}

public async Task<List<Animal>> GetAllAsync(
    AnimalStatus? status,
    CancellationToken cancellationToken = default)
{
    var query = _context.Animals
        .Include(a => a.Shelter)
        .AsQueryable();

    if (status.HasValue)
    {
        query = query.Where(a => a.Status == status.Value);
    }

    return await query.ToListAsync(cancellationToken);
}

   }