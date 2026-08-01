using MediatR;
using System.IO;

namespace TierMatch.Application.Animals.Commands.UploadAnimalImage;

public sealed record UploadAnimalImageCommand(
    Guid AnimalId,
    Stream Stream,
    string FileName,
    string ContentType,
    long FileSize
) : IRequest<Guid>;