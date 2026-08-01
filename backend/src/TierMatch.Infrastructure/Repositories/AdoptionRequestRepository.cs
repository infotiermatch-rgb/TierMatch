using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public class AdoptionRequestRepository
    : IAdoptionRequestRepository
{
    private readonly AppDbContext _context;

    public AdoptionRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdoptionRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(x => x.Animal)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(x => x.Animal)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Where(x => x.AnimalId == animalId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByStatusAsync(
        AdoptionRequestStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        AdoptionRequest request,
        CancellationToken cancellationToken = default)
    {
        await _context.AdoptionRequests.AddAsync(
            request,
            cancellationToken);
    }

    public void Update(
        AdoptionRequest request)
    {
        _context.AdoptionRequests.Update(request);
    }

    public void Delete(
        AdoptionRequest request)
    {
        _context.AdoptionRequests.Remove(request);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<AdoptionRequest>> GetPendingByAnimalIdAsync(
    Guid animalId,
    CancellationToken cancellationToken = default)
{
    return await _context.AdoptionRequests
        .Where(x =>
            x.AnimalId == animalId &&
            x.Status == AdoptionRequestStatus.Pending)
        .ToListAsync(cancellationToken);
}
}