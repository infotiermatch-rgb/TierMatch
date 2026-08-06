using TierMatch.Domain.Common;
using TierMatch.Domain.Enums;

namespace TierMatch.Domain.Entities;

public class ShelterRegistration : BaseEntity
{
    /*
     * Stammdaten des Tierheims
     */
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
        "DE";

    public string ShelterPhoneNumber { get; set; } =
        string.Empty;

    public string ShelterEmail { get; set; } =
        string.Empty;

    public string Website { get; set; } =
        string.Empty;

    public string Description { get; set; } =
        string.Empty;

    /*
     * Verantwortlicher Ansprechpartner
     */
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

    /*
     * Prüfungs- und Freigabestatus
     */
    public ShelterRegistrationStatus Status
    {
        get;
        set;
    } = ShelterRegistrationStatus.Pending;

    public string RejectionReason { get; set; } =
        string.Empty;

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    /*
     * Werden erst nach erfolgreicher Genehmigung
     * gesetzt.
     */
    public Guid? ShelterId { get; set; }

    public Guid? UserId { get; set; }
}