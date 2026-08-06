using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Application.ShelterRegistrations.DTOs;

namespace TierMatch.Application.ShelterRegistrations.Queries.GetShelterRegistrations;

public sealed class GetShelterRegistrationsQueryHandler
    : IRequestHandler<
        GetShelterRegistrationsQuery,
        Result<List<ShelterRegistrationListItemDto>>>
{
    private readonly
        IShelterRegistrationRepository
        _repository;

    private readonly ICurrentUserService
        _currentUserService;

    public GetShelterRegistrationsQueryHandler(
        IShelterRegistrationRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService =
            currentUserService;
    }

    public async Task<
        Result<List<ShelterRegistrationListItemDto>>>
        Handle(
            GetShelterRegistrationsQuery request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<
                List<ShelterRegistrationListItemDto>>
                .Unauthorized();
        }

        if (!_currentUserService.IsInRole(
                Roles.Admin))
        {
            return Result<
                List<ShelterRegistrationListItemDto>>
                .Forbidden();
        }

        var registrations =
            await _repository.GetAllAsync(
                request.Status,
                cancellationToken);

        var response =
            registrations
                .Select(registration =>
                    new ShelterRegistrationListItemDto
                    {
                        Id = registration.Id,

                        ShelterName =
                            registration.ShelterName,

                        City =
                            registration.City,

                        ShelterEmail =
                            registration.ShelterEmail,

                        ContactFirstName =
                            registration.ContactFirstName,

                        ContactLastName =
                            registration.ContactLastName,

                        ContactEmail =
                            registration.ContactEmail,

                        Status =
                            registration.Status,

                        CreatedAt =
                            registration.CreatedAt,

                        ReviewedAt =
                            registration.ReviewedAt
                    })
                .ToList();

        return Result<
            List<ShelterRegistrationListItemDto>>
            .Success(response);
    }
}