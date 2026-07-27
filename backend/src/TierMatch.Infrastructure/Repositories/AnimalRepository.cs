using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
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
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Animal>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Animals
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

   }