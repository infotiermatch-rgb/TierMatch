using MediatR;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.ShelterRegistrations.Commands.ApproveShelterRegistration;

public sealed class ApproveShelterRegistrationHandler
    : IRequestHandler<
        ApproveShelterRegistrationCommand,
        Result<ApproveShelterRegistrationResponse>>
{
    private readonly
        IShelterRegistrationRepository
        _registrationRepository;

    private readonly IShelterRepository
        _shelterRepository;

    private readonly IIdentityService
        _identityService;

    private readonly IEmailService
        _emailService;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IUnitOfWork
        _unitOfWork;

    public ApproveShelterRegistrationHandler(
        IShelterRegistrationRepository
            registrationRepository,
        IShelterRepository shelterRepository,
        IIdentityService identityService,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _registrationRepository =
            registrationRepository;

        _shelterRepository =
            shelterRepository;

        _identityService =
            identityService;

        _emailService =
            emailService;

        _currentUserService =
            currentUserService;

        _unitOfWork =
            unitOfWork;
    }

    public async Task<
        Result<ApproveShelterRegistrationResponse>>
        Handle(
            ApproveShelterRegistrationCommand request,
            CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<
                ApproveShelterRegistrationResponse>
                .Unauthorized();
        }

        if (!_currentUserService.IsInRole(
                Roles.Admin))
        {
            return Result<
                ApproveShelterRegistrationResponse>
                .Forbidden();
        }

        if (request.Id == Guid.Empty)
        {
            return Result<
                ApproveShelterRegistrationResponse>
                .Validation(
                    "Es wurde keine gültige Registrierungs-ID angegeben.");
        }

        var reviewedByUserId =
            _currentUserService.UserId;

        if (
            reviewedByUserId is null ||
            reviewedByUserId == Guid.Empty)
        {
            return Result<
                ApproveShelterRegistrationResponse>
                .Unauthorized();
        }

        var recipientEmail =
            string.Empty;

        var recipientName =
            string.Empty;

        var shelterName =
            string.Empty;

        var setupToken =
            string.Empty;

        var shelterId =
            Guid.Empty;

        var userId =
            Guid.Empty;

        await using (
            var transaction =
                await _unitOfWork
                    .BeginTransactionAsync(
                        cancellationToken))
        {
            try
            {
                var registration =
                    await _registrationRepository
                        .GetByIdAsync(
                            request.Id,
                            cancellationToken);

                if (registration is null)
                {
                    await transaction.RollbackAsync(
                        CancellationToken.None);

                    return Result<
                        ApproveShelterRegistrationResponse>
                        .NotFound(
                            "Die Tierheimregistrierung wurde nicht gefunden.");
                }

                if (
                    registration.Status !=
                    ShelterRegistrationStatus.Pending)
                {
                    await transaction.RollbackAsync(
                        CancellationToken.None);

                    return Result<
                        ApproveShelterRegistrationResponse>
                        .Validation(
                            "Die Tierheimregistrierung wurde bereits bearbeitet.");
                }

                var shelter =
                    new Shelter
                    {
                        Name =
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

                        PhoneNumber =
                            registration
                                .ShelterPhoneNumber,

                        Email =
                            registration.ShelterEmail,

                        Website =
                            registration.Website,

                        Description =
                            registration.Description,

                        CreatedAt =
                            DateTime.UtcNow
                    };

                await _shelterRepository.AddAsync(
                    shelter,
                    cancellationToken);

                /*
                 * Das Tierheim muss zuerst innerhalb
                 * der Transaktion gespeichert werden.
                 *
                 * Der IdentityService prüft anschließend,
                 * ob die erzeugte ShelterId existiert.
                 */
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                var accountResult =
                    await _identityService
                        .CreateShelterAdminAccountAsync(
                            registration.ContactFirstName,
                            registration.ContactLastName,
                            registration.ContactEmail,
                            shelter.Id,
                            cancellationToken);

                if (!accountResult.IsSuccess)
                {
                    await transaction.RollbackAsync(
                        CancellationToken.None);

                    return MapIdentityFailure(
                        accountResult);
                }

                var accountSetup =
                    accountResult.Value;

                if (accountSetup is null)
                {
                    await transaction.RollbackAsync(
                        CancellationToken.None);

                    return Result<
                        ApproveShelterRegistrationResponse>
                        .Conflict(
                            "Das Tierheimkonto konnte nicht vollständig erstellt werden.");
                }

                var reviewedAt =
                    DateTime.UtcNow;

                registration.Status =
                    ShelterRegistrationStatus.Approved;

                registration.RejectionReason =
                    string.Empty;

                registration.ReviewedAt =
                    reviewedAt;

                registration.ReviewedByUserId =
                    reviewedByUserId.Value;

                registration.ShelterId =
                    shelter.Id;

                registration.UserId =
                    accountSetup.UserId;

                registration.UpdatedAt =
                    reviewedAt;

                _registrationRepository.Update(
                    registration);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);

                recipientEmail =
                    registration.ContactEmail;

                recipientName =
                    string.Join(
                        " ",
                        new[]
                        {
                            registration.ContactFirstName,
                            registration.ContactLastName
                        }
                        .Where(
                            value =>
                                !string.IsNullOrWhiteSpace(
                                    value)));

                shelterName =
                    registration.ShelterName;

                setupToken =
                    accountSetup.SetupToken;

                shelterId =
                    shelter.Id;

                userId =
                    accountSetup.UserId;
            }
            catch (OperationCanceledException)
                when (
                    cancellationToken
                        .IsCancellationRequested)
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                throw;
            }
            catch
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                throw;
            }
        }

        var setupEmailSent =
            false;

        try
        {
            await _emailService
                .SendShelterAccountSetupEmailAsync(
                    recipientEmail,
                    recipientName,
                    shelterName,
                    setupToken,
                    cancellationToken);

            setupEmailSent = true;
        }
        catch (OperationCanceledException)
            when (
                cancellationToken
                    .IsCancellationRequested)
        {
            /*
             * Die Datenbanktransaktion wurde bereits
             * erfolgreich abgeschlossen.
             *
             * Eine abgebrochene E-Mail darf die
             * Genehmigung deshalb nicht rückgängig
             * machen.
             */
            setupEmailSent = false;
        }
        catch
        {
            /*
             * Der E-Mail-Dienst protokolliert den
             * eigentlichen Versandfehler.
             *
             * Das Tierheim und das Konto bleiben
             * genehmigt. Später kann ein erneuter
             * Versand angeboten werden.
             */
            setupEmailSent = false;
        }

        var message =
            setupEmailSent
                ? "Die Tierheimregistrierung wurde genehmigt und die Einrichtungs-E-Mail wurde versendet."
                : "Die Tierheimregistrierung wurde genehmigt. Die Einrichtungs-E-Mail konnte jedoch nicht versendet werden.";

        return Result<
            ApproveShelterRegistrationResponse>
            .Success(
                new ApproveShelterRegistrationResponse(
                    request.Id,
                    shelterId,
                    userId,
                    setupEmailSent,
                    message));
    }

    private static Result<
        ApproveShelterRegistrationResponse>
        MapIdentityFailure(
            Result<
                ShelterAdminAccountSetupResponse>
                accountResult)
    {
        var message =
            accountResult.Error.Message;

        return accountResult.Status switch
        {
            ResultStatus.Validation =>
                Result<
                    ApproveShelterRegistrationResponse>
                    .Validation(message),

            ResultStatus.NotFound =>
                Result<
                    ApproveShelterRegistrationResponse>
                    .NotFound(message),

            ResultStatus.Conflict =>
                Result<
                    ApproveShelterRegistrationResponse>
                    .Conflict(message),

            ResultStatus.Unauthorized =>
                Result<
                    ApproveShelterRegistrationResponse>
                    .Unauthorized(),

            ResultStatus.Forbidden =>
                Result<
                    ApproveShelterRegistrationResponse>
                    .Forbidden(),

            _ =>
                Result<
                    ApproveShelterRegistrationResponse>
                    .Conflict(
                        "Das Tierheimkonto konnte nicht erstellt werden.")
        };
    }
}