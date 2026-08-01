namespace TierMatch.Application.AdoptionRequests.DTOs;

public class CreateAdoptionRequestDto
{
    public Guid AnimalId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}