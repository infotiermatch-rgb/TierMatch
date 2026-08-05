using Microsoft.Extensions.Logging;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Infrastructure.Authentication;

public sealed partial class IdentityService
{
    public async Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return Result.Validation(
                "Die E-Mail-Adresse darf nicht leer sein.");
        }

        var email =
            request.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(
                email);

        /*
         * Es wird absichtlich auch dann NoContent zurückgegeben,
         * wenn kein Benutzer existiert oder dieser deaktiviert ist.
         *
         * Dadurch kann der Endpunkt nicht zum Ermitteln
         * registrierter E-Mail-Adressen verwendet werden.
         */
        if (user is null ||
            !user.IsActive ||
            string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogInformation(
                "Eine Passwortzurücksetzung wurde für eine " +
                "unbekannte oder nicht verfügbare Adresse angefordert.");

            return Result.NoContent();
        }

        var resetToken =
            await _userManager
                .GeneratePasswordResetTokenAsync(
                    user);

        await _emailService
            .SendPasswordResetEmailAsync(
                recipientEmail: user.Email,
                recipientName: user.FirstName,
                resetToken: resetToken,
                cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Für Benutzer {UserId} wurde eine " +
            "Passwortzurücksetzung angefordert.",
            user.Id);

        return Result.NoContent();
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            return Result.Validation(
                "Es wurden keine Daten zur Passwortzurücksetzung übermittelt.");
        }

        var email =
            request.Email?.Trim() ??
            string.Empty;

        var token =
            request.Token ??
            string.Empty;

        var newPassword =
            request.NewPassword ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Validation(
                "Die E-Mail-Adresse darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Validation(
                "Der Token zur Passwortzurücksetzung fehlt.");
        }

        /*
         * Das Passwort wird nicht getrimmt, da Leerzeichen
         * grundsätzlich Bestandteil eines Passworts sein können.
         */
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result.Validation(
                "Das neue Passwort darf nicht leer sein.");
        }

        var user =
            await _userManager.FindByEmailAsync(
                email);

        /*
         * Für unbekannte oder deaktivierte Benutzer wird dieselbe
         * Fehlermeldung wie für einen ungültigen Token verwendet.
         */
        if (user is null ||
            !user.IsActive)
        {
            _logger.LogWarning(
                "Eine Passwortzurücksetzung mit ungültigen " +
                "oder nicht verfügbaren Benutzerdaten wurde versucht.");

            return Result.Validation(
                "Der Link zur Passwortzurücksetzung ist ungültig oder abgelaufen.");
        }

        var resetResult =
            await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

        if (!resetResult.Succeeded)
        {
            var invalidToken =
                resetResult.Errors.Any(
                    error =>
                        string.Equals(
                            error.Code,
                            "InvalidToken",
                            StringComparison.OrdinalIgnoreCase));

            if (invalidToken)
            {
                _logger.LogWarning(
                    "Für Benutzer {UserId} wurde ein ungültiger " +
                    "Passwort-Reset-Token verwendet.",
                    user.Id);

                return Result.Validation(
                    "Der Link zur Passwortzurücksetzung ist ungültig oder abgelaufen.");
            }

            var errors =
                FormatIdentityErrors(
                    resetResult);

            _logger.LogWarning(
                "Das Passwort von Benutzer {UserId} konnte nicht " +
                "zurückgesetzt werden. Fehler: {Errors}",
                user.Id,
                errors);

            return Result.Validation(errors);
        }

        /*
         * Alle bestehenden Refresh Tokens werden nach dem
         * Zurücksetzen des Passworts widerrufen.
         */
        await RevokeActiveRefreshTokensAsync(
            user.Id,
            cancellationToken);

        _logger.LogInformation(
            "Das Passwort von Benutzer {UserId} wurde zurückgesetzt. " +
            "Alle aktiven Refresh Tokens wurden widerrufen.",
            user.Id);

        return Result.NoContent();
    }

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
            request.CurrentPassword ??
            string.Empty;

        var newPassword =
            request.NewPassword ??
            string.Empty;

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

        var user =
            userResult.Value!;

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