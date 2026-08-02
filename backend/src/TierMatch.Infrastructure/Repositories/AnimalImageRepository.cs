using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public class AnimalImageRepository : IAnimalImageRepository
{
    private readonly AppDbContext _context;

    public AnimalImageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnimalImage>> GetByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnimalImages
            .Where(i => i.AnimalId == animalId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<AnimalImage?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnimalImages
            .FirstOrDefaultAsync(
                i => i.Id == id,
                cancellationToken);
    }

    public async Task<AnimalImage?> GetPrimaryAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnimalImages
            .FirstOrDefaultAsync(
                i => i.AnimalId == animalId &&
                     i.IsPrimary,
                cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        var maxSortOrder = await _context.AnimalImages
            .Where(i => i.AnimalId == animalId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? 0) + 1;
    }

    public async Task AddAsync(
        AnimalImage image,
        CancellationToken cancellationToken = default)
    {
        await _context.AnimalImages.AddAsync(
            image,
            cancellationToken);
    }

    public void Update(
        AnimalImage image)
    {
        _context.AnimalImages.Update(image);
    }

    public void Delete(
        AnimalImage image)
    {
        _context.AnimalImages.Remove(image);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnimalImages
            .AnyAsync(
                i => i.Id == id,
                cancellationToken);
    }
}