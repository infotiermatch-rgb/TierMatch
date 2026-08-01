using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public class UpdateAnimalCommand : IRequest<Result>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public AnimalSpecies Species { get; set; }

    public string Breed { get; set; } = string.Empty;

    public AnimalGender Gender { get; set; }

    public AnimalSize Size { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsVaccinated { get; set; }

    public bool IsCastrated { get; set; }

    public AnimalStatus Status { get; set; }

    public Guid? ShelterId { get; set; }
}