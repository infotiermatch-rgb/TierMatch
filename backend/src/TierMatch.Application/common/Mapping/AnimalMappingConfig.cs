using Mapster;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mapping;

public static class AnimalMappingConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Animal, AnimalDto>
            .NewConfig()
            .Map(dest => dest.Species, src => src.Species.ToString())
            .Map(dest => dest.Gender, src => src.Gender.ToString())
            .Map(dest => dest.Size, src => src.Size.ToString());
    }
}