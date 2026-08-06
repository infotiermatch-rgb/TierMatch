using MediatR;

using TierMatch.Application.Common.Results;

namespace TierMatch.Application.ShelterRegistrations.Commands.RejectShelterRegistration;

public sealed record RejectShelterRegistrationCommand(
    Guid Id,
    string Reason)
    : IRequest<Result>;