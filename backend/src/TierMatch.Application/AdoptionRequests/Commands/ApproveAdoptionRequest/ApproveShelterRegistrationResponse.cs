namespace TierMatch.Application.ShelterRegistrations.Commands.ApproveShelterRegistration;

public sealed record ApproveShelterRegistrationResponse(
    Guid RegistrationId,
    Guid ShelterId,
    Guid UserId,
    bool SetupEmailSent,
    string Message);