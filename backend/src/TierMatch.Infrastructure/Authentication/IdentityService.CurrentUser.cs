using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;
using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Authentication;

public sealed partial class IdentityService
{
    public async Task<Result<CurrentUserResponse>>
        GetCurrentUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var userResult = await GetActiveUserAsync(
            userId);

        if (!userResult.IsSuccess)
        {
            return userResult.Status switch
            {
                ResultStatus.Forbidden =>
                    Result<CurrentUserResponse>.Forbidden(),

                _ =>
                    Result<CurrentUserResponse>.Unauthorized()
            };
        }

        var response = await MapCurrentUserAsync(
            userResult.Value!);

        return Result<CurrentUserResponse>.Success(
            response);
    }

    public async Task<Result<CurrentUserResponse>>
        UpdateCurrentUserProfileAsync(
            Guid userId,
            UpdateCurrentUserProfileRequest request,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result<CurrentUserResponse>.Unauthorized();
        }

        var firstName =
            request.FirstName?.Trim() ?? string.Empty;

        var lastName =
            request.LastName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result<CurrentUserResponse>.Validation(
                "Der Vorname darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return Result<CurrentUserResponse>.Validation(
                "Der Nachname darf nicht leer sein.");
        }

        if (firstName.Length > 100)
        {
            return Result<CurrentUserResponse>.Validation(
                "Der Vorname darf höchstens 100 Zeichen enthalten.");
        }

        if (lastName.Length > 100)
        {
            return Result<CurrentUserResponse>.Validation(
                "Der Nachname darf höchstens 100 Zeichen enthalten.");
        }

        var userResult = await GetActiveUserAsync(
            userId);

        if (!userResult.IsSuccess)
        {
            return userResult.Status switch
            {
                ResultStatus.Forbidden =>
                    Result<CurrentUserResponse>.Forbidden(),

                _ =>
                    Result<CurrentUserResponse>.Unauthorized()
            };
        }

        var user = userResult.Value!;

        /*
         * Die Operation ist idempotent. Sind die Werte bereits
         * identisch, wird das aktuelle Profil zurückgegeben.
         */
        if (user.FirstName == firstName &&
            user.LastName == lastName)
        {
            var unchangedResponse =
                await MapCurrentUserAsync(user);

            return Result<CurrentUserResponse>.Success(
                unchangedResponse);
        }

        var previousFirstName = user.FirstName;
        var previousLastName = user.LastName;

        user.FirstName = firstName;
        user.LastName = lastName;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            user.FirstName = previousFirstName;
            user.LastName = previousLastName;

            _logger.LogError(
                "Das Profil von Benutzer {UserId} konnte nicht " +
                "aktualisiert werden. Fehler: {Errors}",
                userId,
                FormatIdentityErrors(updateResult));

            return Result<CurrentUserResponse>.Conflict(
                "Das Benutzerprofil konnte nicht gespeichert werden.");
        }

        _logger.LogInformation(
            "Benutzer {UserId} hat sein Profil aktualisiert.",
            userId);

        var response = await MapCurrentUserAsync(
            user);

        return Result<CurrentUserResponse>.Success(
            response);
    }

    private async Task<Result<ApplicationUser>>
        GetActiveUserAsync(
            Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result<ApplicationUser>.Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            _logger.LogWarning(
                "Für die Benutzer-ID {UserId} wurde kein Benutzer gefunden.",
                userId);

            return Result<ApplicationUser>.Unauthorized();
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Der deaktivierte Benutzer {UserId} hat versucht, " +
                "auf seine Profildaten zuzugreifen.",
                userId);

            return Result<ApplicationUser>.Forbidden();
        }

        return Result<ApplicationUser>.Success(user);
    }

    private async Task<CurrentUserResponse>
        MapCurrentUserAsync(
            ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(
            user);

        var orderedRoles = roles
            .OrderBy(role => role)
            .ToList()
            .AsReadOnly();

        return new CurrentUserResponse(
            UserId: user.Id,
            Email: user.Email ?? string.Empty,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Roles: orderedRoles,
            ShelterId: user.ShelterId,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt);
    }
}