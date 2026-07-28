using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public class ShelterRepository : IShelterRepository
{
    private readonly AppDbContext _context;

    public ShelterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Shelter?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Shelter>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Shelters
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Shelter shelter,
        CancellationToken cancellationToken = default)
    {
        await _context.Shelters.AddAsync(shelter, cancellationToken);
    }

    public void Update(Shelter shelter)
    {
        _context.Shelters.Update(shelter);
    }

    public void Delete(Shelter shelter)
    {
        _context.Shelters.Remove(shelter);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}