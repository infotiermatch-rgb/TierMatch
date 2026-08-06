using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Interfaces;

public interface IShelterRegistrationRepository
{
    Task<ShelterRegistration?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<ShelterRegistration>> GetAllAsync(
        ShelterRegistrationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingRegistrationAsync(
        string contactEmail,
        string shelterEmail,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ShelterRegistration registration,
        CancellationToken cancellationToken = default);

    void Update(
        ShelterRegistration registration);
}