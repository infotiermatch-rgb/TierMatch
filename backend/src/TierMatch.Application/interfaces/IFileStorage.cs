namespace TierMatch.Application.Interfaces;

public interface IFileStorage
{
    Task<(string FileName, string FilePath)> SaveAnimalImageAsync(
        Guid animalId,
        Stream stream,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}