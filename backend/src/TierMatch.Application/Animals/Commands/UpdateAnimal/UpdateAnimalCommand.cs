using MediatR;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

/// <summary>
/// Command zum Aktualisieren eines vorhandenen Tieres.
/// </summary>
public sealed record UpdateAnimalCommand(
    Guid Id,
    string Name,
    AnimalSpecies Species,
    string Breed,
    AnimalGender Gender,
    AnimalSize Size,
    DateOnly? BirthDate,
    string Description,
    bool IsVaccinated,
    bool IsCastrated,
    Guid? ShelterId,
    AnimalStatus Status
) : IRequest<bool>;
