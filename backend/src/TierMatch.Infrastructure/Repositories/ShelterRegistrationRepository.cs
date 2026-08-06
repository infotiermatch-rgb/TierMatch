using Microsoft.EntityFrameworkCore;

using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Infrastructure.Repositories;

public class ShelterRegistrationRepository
    : IShelterRegistrationRepository
{
    private readonly AppDbContext _context;

    public ShelterRegistrationRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<ShelterRegistration?>
        GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        return await _context.ShelterRegistrations
            .FirstOrDefaultAsync(
                registration =>
                    registration.Id == id,
                cancellationToken);
    }

    public async Task<List<ShelterRegistration>>
        GetAllAsync(
            ShelterRegistrationStatus? status = null,
            CancellationToken cancellationToken = default)
    {
        IQueryable<ShelterRegistration> query =
            _context.ShelterRegistrations
                .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(
                registration =>
                    registration.Status ==
                    status.Value);
        }

        return await query
            .OrderByDescending(
                registration =>
                    registration.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool>
        HasPendingRegistrationAsync(
            string contactEmail,
            string shelterEmail,
            CancellationToken cancellationToken = default)
    {
        var normalizedContactEmail =
            contactEmail
                .Trim()
                .ToLower();

        var normalizedShelterEmail =
            shelterEmail
                .Trim()
                .ToLower();

        return await _context
            .ShelterRegistrations
            .AnyAsync(
                registration =>
                    registration.Status ==
                        ShelterRegistrationStatus.Pending &&
                    (
                        registration.ContactEmail
                            .ToLower() ==
                            normalizedContactEmail ||
                        registration.ShelterEmail
                            .ToLower() ==
                            normalizedShelterEmail
                    ),
                cancellationToken);
    }

    public async Task AddAsync(
        ShelterRegistration registration,
        CancellationToken cancellationToken = default)
    {
        await _context.ShelterRegistrations
            .AddAsync(
                registration,
                cancellationToken);
    }

    public void Update(
        ShelterRegistration registration)
    {
        _context.ShelterRegistrations
            .Update(registration);
    }
}