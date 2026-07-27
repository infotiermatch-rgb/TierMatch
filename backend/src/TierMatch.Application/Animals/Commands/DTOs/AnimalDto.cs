namespace TierMatch.Application.Animals.DTOs;

public class AnimalDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Breed { get; init; } = string.Empty;

    public string Species { get; init; } = string.Empty;

    public string Gender { get; init; } = string.Empty;

    public string Size { get; init; } = string.Empty;

    public DateOnly? BirthDate { get; init; }

    public string Description { get; init; } = string.Empty;

    public bool IsVaccinated { get; init; }

    public bool IsCastrated { get; init; }
}