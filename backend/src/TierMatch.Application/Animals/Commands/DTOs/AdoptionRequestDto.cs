using TierMatch.Domain.Enums;

namespace TierMatch.Application.AdoptionRequests.DTOs;

public class AdoptionRequestDto
{
    public Guid Id { get; init; }

    public Guid AnimalId { get; init; }

    public string AnimalName { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public AdoptionRequestStatus Status { get; init; }

    public DateTime RequestedAt { get; init; }
}