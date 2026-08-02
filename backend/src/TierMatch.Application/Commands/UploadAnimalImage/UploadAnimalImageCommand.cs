using MediatR;
using System.IO;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Animals.Commands.UploadAnimalImage;

public sealed record UploadAnimalImageCommand(
    Guid AnimalId,
    Stream Stream,
    string FileName,
    string ContentType,
    long FileSize
) : IRequest<Result<Guid>>;