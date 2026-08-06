using TierMatch.Domain.Enums;

namespace TierMatch.Application.ShelterRegistrations.DTOs;

public sealed class ShelterRegistrationDetailsDto
{
    public Guid Id { get; set; }

    public string ShelterName { get; set; } =
        string.Empty;

    public string Street { get; set; } =
        string.Empty;

    public string HouseNumber { get; set; } =
        string.Empty;

    public string PostalCode { get; set; } =
        string.Empty;

    public string City { get; set; } =
        string.Empty;

    public string Country { get; set; } =
        string.Empty;

    public string ShelterPhoneNumber { get; set; } =
        string.Empty;

    public string ShelterEmail { get; set; } =
        string.Empty;

    public string Website { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    public string ContactFirstName { get; set; } =
        string.Empty;

    public string ContactLastName { get; set; } =
        string.Empty;

    public string ContactEmail { get; set; } =
        string.Empty;

    public string ContactPhoneNumber { get; set; } =
        string.Empty;

    public string Message { get; set; } =
        string.Empty;

    public ShelterRegistrationStatus Status { get; set; }

    public string RejectionReason { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public Guid? ShelterId { get; set; }

    public Guid? UserId { get; set; }
}