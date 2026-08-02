using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mapping;

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
            SortOrder = image.SortOrder,

            // URL für das Frontend
            Url = $"/uploads/animals/{image.AnimalId}/{image.FileName}"
        };
    }

    public static List<AnimalImageDto> ToDto(
        this IEnumerable<AnimalImage> images)
    {
        return images
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ToDto())
            .ToList();
    }
}