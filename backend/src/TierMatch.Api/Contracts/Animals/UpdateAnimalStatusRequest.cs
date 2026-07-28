using TierMatch.Domain.Enums;

namespace TierMatch.Api.Contracts.Animals;

public sealed class UpdateAnimalStatusRequest
{
    public AnimalStatus Status { get; init; }
}