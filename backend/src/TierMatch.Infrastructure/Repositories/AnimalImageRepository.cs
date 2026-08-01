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

    public async Task<AnimalImage?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnimalImages
            .Include(i => i.Animal)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
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

    public async Task<AnimalImage?> GetPrimaryAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AnimalImages
            .FirstOrDefaultAsync(
                i => i.AnimalId == animalId && i.IsPrimary,
                cancellationToken);
    }

    public async Task<int> GetNextSortOrderAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        var max = await _context.AnimalImages
            .Where(i => i.AnimalId == animalId)
            .MaxAsync(
                i => (int?)i.SortOrder,
                cancellationToken);

        return (max ?? 0) + 1;
    }

    public async Task AddAsync(
        AnimalImage image,
        CancellationToken cancellationToken = default)
    {
        await _context.AnimalImages.AddAsync(image, cancellationToken);
    }

    public void Update(AnimalImage image)
    {
        _context.AnimalImages.Update(image);
    }

    public void Delete(AnimalImage image)
    {
        _context.AnimalImages.Remove(image);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AnimalImage>> GetAllByAnimalIdAsync(
    Guid animalId,
    CancellationToken cancellationToken = default)
{
    return await _context.AnimalImages
        .Where(i => i.AnimalId == animalId)
        .ToListAsync(cancellationToken);
}
}