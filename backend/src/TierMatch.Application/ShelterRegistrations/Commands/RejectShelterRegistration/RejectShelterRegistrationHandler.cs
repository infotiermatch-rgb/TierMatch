using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.ShelterRegistrations.Commands.RejectShelterRegistration;

public sealed class RejectShelterRegistrationHandler
    : IRequestHandler<
        RejectShelterRegistrationCommand,
        Result>
{
    private readonly IShelterRegistrationRepository
        _repository;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IUnitOfWork
        _unitOfWork;

    public RejectShelterRegistrationHandler(
        IShelterRegistrationRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RejectShelterRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Unauthorized();
        }

        if (!_currentUserService.IsInRole(
                Roles.Admin))
        {
            return Result.Forbidden();
        }

        var reviewedByUserId =
            _currentUserService.UserId;

        if (
            reviewedByUserId is null ||
            reviewedByUserId == Guid.Empty)
        {
            return Result.Unauthorized();
        }

        if (request.Id == Guid.Empty)
        {
            return Result.Validation(
                "Es wurde keine gültige Registrierungs-ID angegeben.");
        }

        var rejectionReason =
            request.Reason?.Trim() ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(
                rejectionReason))
        {
            return Result.Validation(
                "Bitte geben Sie einen Ablehnungsgrund an.");
        }

        if (rejectionReason.Length > 2000)
        {
            return Result.Validation(
                "Der Ablehnungsgrund darf höchstens 2000 Zeichen enthalten.");
        }

        var registration =
            await _repository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (registration is null)
        {
            return Result.NotFound(
                "Die Tierheimregistrierung wurde nicht gefunden.");
        }

        if (
            registration.Status !=
            ShelterRegistrationStatus.Pending)
        {
            return Result.Validation(
                "Die Tierheimregistrierung wurde bereits bearbeitet.");
        }

        var reviewedAt =
            DateTime.UtcNow;

        registration.Status =
            ShelterRegistrationStatus.Rejected;

        registration.RejectionReason =
            rejectionReason;

        registration.ReviewedAt =
            reviewedAt;

        registration.ReviewedByUserId =
            reviewedByUserId.Value;

        registration.ShelterId = null;
        registration.UserId = null;
        registration.UpdatedAt = reviewedAt;

        _repository.Update(
            registration);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.NoContent();
    }
}