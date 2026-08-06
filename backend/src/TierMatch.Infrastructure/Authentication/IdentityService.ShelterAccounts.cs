using Microsoft.Extensions.Logging;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Authentication;

public sealed partial class IdentityService
{
    public async Task<
        Result<ShelterAdminAccountSetupResponse>>
        CreateShelterAdminAccountAsync(
            string firstName,
            string lastName,
            string email,
            Guid shelterId,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedFirstName =
            firstName?.Trim() ??
            string.Empty;

        var normalizedLastName =
            lastName?.Trim() ??
            string.Empty;

        var normalizedEmail =
            email?.Trim() ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(
                normalizedFirstName))
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .Validation(
                    "Der Vorname des Ansprechpartners ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(
                normalizedLastName))
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .Validation(
                    "Der Nachname des Ansprechpartners ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(
                normalizedEmail))
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .Validation(
                    "Die E-Mail-Adresse des Ansprechpartners ist erforderlich.");
        }

        if (shelterId == Guid.Empty)
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .Validation(
                    "Es wurde keine gültige Tierheim-ID angegeben.");
        }

        var shelter =
            await _shelterRepository.GetByIdAsync(
                shelterId,
                cancellationToken);

        if (shelter is null)
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .NotFound(
                    "Das zugehörige Tierheim wurde nicht gefunden.");
        }

        var existingUser =
            await _userManager.FindByEmailAsync(
                normalizedEmail);

        if (existingUser is not null)
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .Conflict(
                    "Ein Benutzer mit dieser E-Mail-Adresse existiert bereits.");
        }

        var user =
            new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,

                FirstName =
                    normalizedFirstName,

                LastName =
                    normalizedLastName,

                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = null,
                ShelterId = shelterId
            };

        /*
         * Das Konto wird bewusst ohne Passwort
         * erstellt. Das Passwort wird später über
         * den Einrichtungslink festgelegt.
         */
        var createResult =
            await _userManager.CreateAsync(
                user);

        if (!createResult.Succeeded)
        {
            return Result<
                ShelterAdminAccountSetupResponse>
                .Validation(
                    FormatIdentityErrors(
                        createResult));
        }

        /*
         * Neue Tierheimkonten erhalten sowohl die
         * Basisrolle User als auch ShelterAdmin.
         *
         * Das entspricht einem regulären Konto mit
         * zusätzlichen Verwaltungsrechten.
         */
        var roleResult =
            await _userManager.AddToRolesAsync(
                user,
                new[]
                {
                    Roles.User,
                    Roles.ShelterAdmin
                });

        if (!roleResult.Succeeded)
        {
            await DeleteIncompleteShelterUserAsync(
                user);

            return Result<
                ShelterAdminAccountSetupResponse>
                .Conflict(
                    "Die Rollen für das Tierheimkonto konnten nicht zugewiesen werden.");
        }

        try
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            var setupToken =
                await _userManager
                    .GeneratePasswordResetTokenAsync(
                        user);

            if (string.IsNullOrWhiteSpace(
                    setupToken))
            {
                await DeleteIncompleteShelterUserAsync(
                    user);

                return Result<
                    ShelterAdminAccountSetupResponse>
                    .Conflict(
                        "Der Einrichtungslink für das Tierheimkonto konnte nicht erzeugt werden.");
            }

            _logger.LogInformation(
                "Das Tierheimkonto {UserId} für Tierheim " +
                "{ShelterId} wurde ohne Startpasswort erstellt.",
                user.Id,
                shelterId);

            return Result<
                ShelterAdminAccountSetupResponse>
                .Success(
                    new ShelterAdminAccountSetupResponse(
                        user.Id,
                        setupToken));
        }
        catch (OperationCanceledException)
            when (
                cancellationToken
                    .IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Der Einrichtungstoken für das " +
                "Tierheimkonto {UserId} konnte nicht " +
                "erzeugt werden.",
                user.Id);

            await DeleteIncompleteShelterUserAsync(
                user);

            return Result<
                ShelterAdminAccountSetupResponse>
                .Conflict(
                    "Das Tierheimkonto konnte nicht vollständig eingerichtet werden.");
        }
    }

    private async Task
        DeleteIncompleteShelterUserAsync(
            ApplicationUser user)
    {
        var deleteResult =
            await _userManager.DeleteAsync(
                user);

        if (deleteResult.Succeeded)
        {
            return;
        }

        _logger.LogError(
            "Das unvollständige Tierheimkonto " +
            "{UserId} konnte nicht gelöscht werden. " +
            "Fehler: {Errors}",
            user.Id,
            FormatIdentityErrors(
                deleteResult));
    }
}