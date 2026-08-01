namespace TierMatch.Application.Animals.DTOs;

public class AnimalImageDto
{
    public Guid Id { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSize { get; init; }

    public bool IsPrimary { get; init; }

    public int SortOrder { get; init; }

    public string Url { get; init; } = string.Empty;
}