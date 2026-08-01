using TierMatch.Domain.Common;
using TierMatch.Domain.Enums;

namespace TierMatch.Domain.Entities;

public class AdoptionRequest : BaseEntity
{
    public Guid AnimalId { get; set; }

    public Animal Animal { get; set; } = null!;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public AdoptionRequestStatus Status { get; set; }
        = AdoptionRequestStatus.Pending;

    public DateTime RequestedAt { get; set; }
        = DateTime.UtcNow;
}