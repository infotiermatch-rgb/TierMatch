using Microsoft.EntityFrameworkCore;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public class AdoptionRequestRepository : IAdoptionRequestRepository
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
            .Include(r => r.Animal)
            .FirstOrDefaultAsync(
                r => r.Id == id,
                cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(r => r.Animal)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(r => r.Animal)
            .Where(r => r.AnimalId == animalId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByStatusAsync(
        AdoptionRequestStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(r => r.Animal)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetPendingByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Where(r =>
                r.AnimalId == animalId &&
                r.Status == AdoptionRequestStatus.Pending)
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

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AnyAsync(
                r => r.Id == id,
                cancellationToken);
    }
}