using Microsoft.AspNetCore.Http;

namespace TierMatch.Api.Contracts.Animals;

public sealed class UploadAnimalImageRequest
{
    public IFormFile File { get; init; } =
        default!;
}