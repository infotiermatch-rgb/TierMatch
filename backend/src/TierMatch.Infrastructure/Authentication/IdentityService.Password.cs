using Microsoft.Extensions.Logging;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Infrastructure.Authentication;

public sealed partial class IdentityService
{
    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            return Result.Validation(
                "Es wurden keine Daten zur Passwortänderung übermittelt.");
        }

        var currentPassword =
            request.CurrentPassword ?? string.Empty;

        var newPassword =
            request.NewPassword ?? string.Empty;

        /*
         * Passwörter werden absichtlich nicht getrimmt.
         * Leerzeichen können ein gültiger Bestandteil eines
         * Passworts sein.
         */
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            return Result.Validation(
                "Das aktuelle Passwort darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.Validation(
                "Das neue Passwort darf nicht leer sein.");
        }

        var userResult =
            await GetActiveUserAsync(userId);

        if (!userResult.IsSuccess)
        {
            return userResult.Status switch
            {
                ResultStatus.Forbidden =>
                    Result.Forbidden(),

                _ =>
                    Result.Unauthorized()
            };
        }

        var user = userResult.Value!;

        var currentPasswordIsValid =
            await _userManager.CheckPasswordAsync(
                user,
                currentPassword);

        if (!currentPasswordIsValid)
        {
            _logger.LogWarning(
                "Benutzer {UserId} hat versucht, sein Passwort " +
                "mit einem falschen aktuellen Passwort zu ändern.",
                userId);

            return Result.Validation(
                "Das aktuelle Passwort ist nicht korrekt.");
        }

        if (string.Equals(
                currentPassword,
                newPassword,
                StringComparison.Ordinal))
        {
            return Result.Validation(
                "Das neue Passwort muss sich vom aktuellen Passwort unterscheiden.");
        }

        var changePasswordResult =
            await _userManager.ChangePasswordAsync(
                user,
                currentPassword,
                newPassword);

        if (!changePasswordResult.Succeeded)
        {
            var errors =
                FormatIdentityErrors(
                    changePasswordResult);

            _logger.LogWarning(
                "Das Passwort von Benutzer {UserId} konnte nicht " +
                "geändert werden. Fehler: {Errors}",
                userId,
                errors);

            return Result.Validation(errors);
        }

        /*
         * Nach einer Passwortänderung werden alle Refresh Tokens
         * widerrufen. Dadurch können bestehende Sitzungen keine
         * neuen Access Tokens mehr anfordern.
         */
        await RevokeActiveRefreshTokensAsync(
            user.Id,
            cancellationToken);

        _logger.LogInformation(
            "Benutzer {UserId} hat sein Passwort geändert. " +
            "Alle aktiven Refresh Tokens wurden widerrufen.",
            userId);

        return Result.NoContent();
    }
}