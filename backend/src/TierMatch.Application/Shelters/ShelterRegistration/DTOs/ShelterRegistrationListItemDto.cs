using TierMatch.Domain.Enums;

namespace TierMatch.Application.ShelterRegistrations.DTOs;

public sealed class ShelterRegistrationListItemDto
{
    public Guid Id { get; set; }

    public string ShelterName { get; set; } =
        string.Empty;

    public string City { get; set; } =
        string.Empty;

    public string ShelterEmail { get; set; } =
        string.Empty;

    public string ContactFirstName { get; set; } =
        string.Empty;

    public string ContactLastName { get; set; } =
        string.Empty;

    public string ContactEmail { get; set; } =
        string.Empty;

    public ShelterRegistrationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }
}