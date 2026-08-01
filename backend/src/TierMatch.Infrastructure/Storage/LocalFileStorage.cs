using TierMatch.Application.Interfaces;

namespace TierMatch.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _uploadRoot;

    public LocalFileStorage()
    {
        _uploadRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "uploads",
            "animals");
    }

    public async Task<(string FileName, string FilePath)> SaveAnimalImageAsync(
        Guid animalId,
        Stream stream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var animalFolder = Path.Combine(
            _uploadRoot,
            animalId.ToString());

        Directory.CreateDirectory(animalFolder);

        var extension = Path.GetExtension(originalFileName);

        var uniqueName = $"{Guid.NewGuid()}{extension}";

        var fullPath = Path.Combine(
            animalFolder,
            uniqueName);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create);

        await stream.CopyToAsync(fileStream, cancellationToken);

        // Relativen Pfad zurückgeben
        var relativePath = Path.Combine(
            "uploads",
            "animals",
            animalId.ToString(),
            uniqueName);

            relativePath = relativePath.Replace("\\", "/");

        return (originalFileName, relativePath);
    }

public Task DeleteAsync(
    string filePath,
    CancellationToken cancellationToken = default)
{
    var fullPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

    if (File.Exists(fullPath))
    {
        File.Delete(fullPath);
    }

    return Task.CompletedTask;
}
}