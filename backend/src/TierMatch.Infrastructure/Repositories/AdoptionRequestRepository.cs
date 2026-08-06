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

    public AdoptionRequestRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdoptionRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(request => request.Animal)
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken);
    }

    public async Task<AdoptionRequest?> GetByIdAndShelterIdAsync(
        Guid id,
        Guid shelterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .Include(request => request.Animal)
            .FirstOrDefaultAsync(
                request =>
                    request.Id == id &&
                    request.Animal.ShelterId == shelterId,
                cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AsNoTracking()
            .Include(request => request.Animal)
            .OrderByDescending(
                request => request.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByShelterIdAsync(
        Guid shelterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AsNoTracking()
            .Include(request => request.Animal)
            .Where(
                request =>
                    request.Animal.ShelterId == shelterId)
            .OrderByDescending(
                request => request.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AsNoTracking()
            .Include(request => request.Animal)
            .Where(
                request =>
                    request.AnimalId == animalId)
            .OrderByDescending(
                request => request.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AsNoTracking()
            .Include(request => request.Animal)
            .Where(
                request =>
                    request.UserId == userId)
            .OrderByDescending(
                request => request.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetByStatusAsync(
        AdoptionRequestStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AsNoTracking()
            .Include(request => request.Animal)
            .Where(
                request =>
                    request.Status == status)
            .OrderByDescending(
                request => request.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AdoptionRequest>> GetPendingByAnimalIdAsync(
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        /*
         * Kein AsNoTracking(), da diese Datensätze beim
         * Genehmigen einer anderen Anfrage verändert werden.
         */
        return await _context.AdoptionRequests
            .Where(
                request =>
                    request.AnimalId == animalId &&
                    request.Status ==
                    AdoptionRequestStatus.Pending)
            .OrderByDescending(
                request => request.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPendingRequestAsync(
        Guid userId,
        Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AdoptionRequests
            .AnyAsync(
                request =>
                    request.UserId == userId &&
                    request.AnimalId == animalId &&
                    request.Status ==
                    AdoptionRequestStatus.Pending,
                cancellationToken);
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
                request => request.Id == id,
                cancellationToken);
    }
}