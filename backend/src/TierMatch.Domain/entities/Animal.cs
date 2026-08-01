using TierMatch.Domain.Common;
using TierMatch.Domain.Enums;

namespace TierMatch.Domain.Entities;

public class Animal : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public AnimalSpecies Species { get; set; }

    public string Breed { get; set; } = string.Empty;

    public AnimalGender Gender { get; set; }

    public AnimalSize Size { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsVaccinated { get; set; }

    public bool IsCastrated { get; set; }

    public AnimalStatus Status { get; set; } = AnimalStatus.Available;

    public Guid? ShelterId { get; set; }

    public Shelter? Shelter { get; set; }

    public ICollection<AnimalImage> Images { get; set; }
    = new List<AnimalImage>();

    public ICollection<AdoptionRequest> AdoptionRequests
    { get; set; }
    = new List<AdoptionRequest>();
}