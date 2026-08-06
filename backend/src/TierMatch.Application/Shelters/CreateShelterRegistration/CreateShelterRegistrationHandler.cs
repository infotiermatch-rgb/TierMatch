using MediatR;

using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.ShelterRegistrations.Commands.CreateShelterRegistration;

public sealed class CreateShelterRegistrationHandler
    : IRequestHandler<
        CreateShelterRegistrationCommand,
        Result<Guid>>
{
    private readonly
        IShelterRegistrationRepository
        _repository;

    private readonly IUnitOfWork _unitOfWork;

    public CreateShelterRegistrationHandler(
        IShelterRegistrationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateShelterRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var contactEmail =
            request.ContactEmail.Trim();

        var shelterEmail =
            request.ShelterEmail.Trim();

        var hasPendingRegistration =
            await _repository
                .HasPendingRegistrationAsync(
                    contactEmail,
                    shelterEmail,
                    cancellationToken);

        if (hasPendingRegistration)
        {
            return Result<Guid>.Conflict(
                "Für diese E-Mail-Adresse liegt bereits eine offene Tierheimregistrierung vor.");
        }

        var registration =
            new ShelterRegistration
            {
                ShelterName =
                    request.ShelterName.Trim(),

                Street =
                    request.Street.Trim(),

                HouseNumber =
                    request.HouseNumber.Trim(),

                PostalCode =
                    request.PostalCode.Trim(),

                City =
                    request.City.Trim(),

                Country =
                    request.Country
                        .Trim()
                        .ToUpperInvariant(),

                ShelterPhoneNumber =
                    request.ShelterPhoneNumber.Trim(),

                ShelterEmail =
                    shelterEmail,

                Website =
                    request.Website.Trim(),

                Description =
                    request.Description.Trim(),

                ContactFirstName =
                    request.ContactFirstName.Trim(),

                ContactLastName =
                    request.ContactLastName.Trim(),

                ContactEmail =
                    contactEmail,

                ContactPhoneNumber =
                    request.ContactPhoneNumber.Trim(),

                Message =
                    request.Message.Trim(),

                Status =
                    ShelterRegistrationStatus.Pending,

                RejectionReason =
                    string.Empty,

                ReviewedAt =
                    null,

                ReviewedByUserId =
                    null,

                ShelterId =
                    null,

                UserId =
                    null
            };

        await _repository.AddAsync(
            registration,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            registration.Id);
    }
}