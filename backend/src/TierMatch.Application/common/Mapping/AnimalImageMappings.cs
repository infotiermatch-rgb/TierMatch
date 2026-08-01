using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mappings;

public static class AnimalImageMappings
{
    public static AnimalImageDto ToDto(this AnimalImage image)
    {
        return new AnimalImageDto
        {
            Id = image.Id,
            FileName = image.FileName,
            FilePath = image.FilePath,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            IsPrimary = image.IsPrimary,
            SortOrder = image.SortOrder
        };
    }
}