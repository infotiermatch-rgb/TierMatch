using MediatR;

namespace TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;

public sealed record CreateAdoptionRequestCommand(
    Guid AnimalId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Message
) : IRequest<Guid>;