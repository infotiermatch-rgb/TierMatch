using MediatR;

using TierMatch.Application.Common.Results;

namespace TierMatch.Application.ShelterRegistrations.Commands.ApproveShelterRegistration;

public sealed record ApproveShelterRegistrationCommand(
    Guid Id)
    : IRequest<
        Result<ApproveShelterRegistrationResponse>>;