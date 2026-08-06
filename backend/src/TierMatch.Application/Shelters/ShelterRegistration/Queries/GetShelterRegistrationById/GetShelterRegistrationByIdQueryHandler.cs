using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Application.ShelterRegistrations.DTOs;

namespace TierMatch.Application.ShelterRegistrations.Queries.GetShelterRegistrationById;

public sealed class GetShelterRegistrationByIdQueryHandler
    : IRequestHandler<
        GetShelterRegistrationByIdQuery,
        Result<ShelterRegistrationDetailsDto>>
{
    private readonly IShelterRegistrationRepository
        _repository;

    private readonly ICurrentUserService
        _currentUserService;

    public GetShelterRegistrationByIdQueryHandler(
        IShelterRegistrationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<
        Result<ShelterRegistrationDetailsDto>>
        Handle(
            GetShelterRegistrationByIdQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<ShelterRegistrationDetailsDto>
                .Unauthorized();
        }

        if (!_currentUserService.IsInRole(
                Roles.Admin))
        {
            return Result<ShelterRegistrationDetailsDto>
                .Forbidden();
        }

        if (request.Id == Guid.Empty)
        {
            return Result<ShelterRegistrationDetailsDto>
                .Validation(
                    "Es wurde keine gültige Registrierungs-ID angegeben.");
        }

        var registration =
            await _repository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (registration is null)
        {
            return Result<ShelterRegistrationDetailsDto>
                .NotFound(
                    "Die Tierheimregistrierung wurde nicht gefunden.");
        }

        var response =
            new ShelterRegistrationDetailsDto
            {
                Id = registration.Id,

                ShelterName =
                    registration.ShelterName,

                Street =
                    registration.Street,

                HouseNumber =
                    registration.HouseNumber,

                PostalCode =
                    registration.PostalCode,

                City =
                    registration.City,

                Country =
                    registration.Country,

                ShelterPhoneNumber =
                    registration.ShelterPhoneNumber,

                ShelterEmail =
                    registration.ShelterEmail,

                Website =
                    registration.Website,

                Description =
                    registration.Description,

                ContactFirstName =
                    registration.ContactFirstName,

                ContactLastName =
                    registration.ContactLastName,

                ContactEmail =
                    registration.ContactEmail,

                ContactPhoneNumber =
                    registration.ContactPhoneNumber,

                Message =
                    registration.Message,

                Status =
                    registration.Status,

                RejectionReason =
                    registration.RejectionReason,

                CreatedAt =
                    registration.CreatedAt,

                UpdatedAt =
                    registration.UpdatedAt,

                ReviewedAt =
                    registration.ReviewedAt,

                ReviewedByUserId =
                    registration.ReviewedByUserId,

                ShelterId =
                    registration.ShelterId,

                UserId =
                    registration.UserId
            };

        return Result<ShelterRegistrationDetailsDto>
            .Success(response);
    }
}