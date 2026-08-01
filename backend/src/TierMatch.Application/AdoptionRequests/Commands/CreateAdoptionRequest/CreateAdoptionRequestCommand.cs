using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;

public sealed record CreateAdoptionRequestCommand(
    Guid AnimalId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Message
) : IRequest<Result<Guid>>;