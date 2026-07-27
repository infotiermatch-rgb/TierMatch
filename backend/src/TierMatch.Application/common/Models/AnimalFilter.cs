using TierMatch.Domain.Enums;

namespace TierMatch.Application.Animals.Models;

public class AnimalFilter
{
    public AnimalSpecies? Species { get; init; }

    public AnimalGender? Gender { get; init; }

    public AnimalSize? Size { get; init; }

    public string? Search { get; init; }
}