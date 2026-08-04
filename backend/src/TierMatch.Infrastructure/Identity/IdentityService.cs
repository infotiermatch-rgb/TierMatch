using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Authentication;

public sealed partial class IdentityService : IIdentityService
{
    private const string DefaultUserRole = Roles.User;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IShelterRepository _shelterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IShelterRepository shelterRepository,
        IUnitOfWork unitOfWork,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _shelterRepository = shelterRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(
            email);

        if (existingUser is not null)
        {
            return Result<AuthenticationResponse>.Conflict(
                "Ein Benutzer mit dieser E-Mail-Adresse existiert bereits.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ShelterId = null
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            return Result<AuthenticationResponse>.Validation(
                FormatIdentityErrors(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            DefaultUserRole);

        if (!roleResult.Succeeded)
        {
            var deleteResult = await _userManager.DeleteAsync(
                user);

            if (!deleteResult.Succeeded)
            {
                _logger.LogError(
                    "Benutzer {UserId} konnte nach fehlgeschlagener " +
                    "Rollenzuweisung nicht gelöscht werden. Fehler: {Errors}",
                    user.Id,
                    FormatIdentityErrors(deleteResult));
            }

            _logger.LogError(
                "Die Rolle {Role} konnte dem neuen Benutzer {UserId} " +
                "nicht zugewiesen werden. Fehler: {Errors}",
                DefaultUserRole,
                user.Id,
                FormatIdentityErrors(roleResult));

            return Result<AuthenticationResponse>.Conflict(
                "Die Standardrolle konnte nicht zugewiesen werden.");
        }

        var authentication = await CreateSessionAsync(
            user,
            refreshTokenToReplace: null,
            cancellationToken);

        _logger.LogInformation(
            "Benutzer {UserId} wurde erfolgreich registriert.",
            user.Id);

        return Result<AuthenticationResponse>.Success(
            authentication);
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();

        var user = await _userManager.FindByEmailAsync(
            email);

        if (user is null)
        {
            _logger.LogWarning(
                "Fehlgeschlagener Loginversuch für eine unbekannte " +
                "E-Mail-Adresse.");

            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Der deaktivierte Benutzer {UserId} hat versucht, " +
                "sich anzumelden.",
                user.Id);

            return Result<AuthenticationResponse>.Forbidden();
        }

        var signInResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning(
                "Der Benutzer {UserId} ist aufgrund zu vieler " +
                "fehlgeschlagener Anmeldeversuche gesperrt.",
                user.Id);

            return Result<AuthenticationResponse>.Forbidden();
        }

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning(
                "Fehlgeschlagener Loginversuch für Benutzer {UserId}.",
                user.Id);

            return Result<AuthenticationResponse>.Unauthorized();
        }

        user.LastLoginAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(
            user);

        if (!updateResult.Succeeded)
        {
            _logger.LogError(
                "Der letzte Loginzeitpunkt von Benutzer {UserId} " +
                "konnte nicht gespeichert werden. Fehler: {Errors}",
                user.Id,
                FormatIdentityErrors(updateResult));

            return Result<AuthenticationResponse>.Conflict(
                "Die Anmeldung konnte nicht vollständig verarbeitet werden.");
        }

        var authentication = await CreateSessionAsync(
            user,
            refreshTokenToReplace: null,
            cancellationToken);

        _logger.LogInformation(
            "Benutzer {UserId} hat sich erfolgreich angemeldet.",
            user.Id);

        return Result<AuthenticationResponse>.Success(
            authentication);
    }

    public async Task<Result<AuthenticationResponse>> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthenticationResponse>.Validation(
                "Der Refresh Token darf nicht leer sein.");
        }

        var storedRefreshToken =
            await _refreshTokenRepository.GetByTokenAsync(
                request.RefreshToken,
                cancellationToken);

        if (storedRefreshToken is null)
        {
            _logger.LogWarning(
                "Es wurde versucht, einen unbekannten Refresh Token " +
                "zu verwenden.");

            return Result<AuthenticationResponse>.Unauthorized();
        }

        /*
         * Die erneute Verwendung eines widerrufenen Refresh Tokens
         * kann auf einen gestohlenen Token hinweisen.
         *
         * Deshalb werden alle aktiven Sitzungen des Benutzers
         * vorsorglich widerrufen.
         */
        if (storedRefreshToken.IsRevoked)
        {
            var activeTokens =
                await _refreshTokenRepository
                    .GetActiveTokensByUserIdAsync(
                        storedRefreshToken.UserId,
                        cancellationToken);

            foreach (var activeToken in activeTokens)
            {
                activeToken.Revoke(
                    replacedByTokenHash: null,
                    revokedByIp: null);
            }

            if (activeTokens.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);
            }

            _logger.LogWarning(
                "Ein bereits widerrufener Refresh Token {RefreshTokenId} " +
                "von Benutzer {UserId} wurde erneut verwendet. " +
                "{TokenCount} aktive Sitzungen wurden widerrufen.",
                storedRefreshToken.Id,
                storedRefreshToken.UserId,
                activeTokens.Count);

            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (storedRefreshToken.IsExpired)
        {
            _logger.LogInformation(
                "Der abgelaufene Refresh Token {RefreshTokenId} " +
                "von Benutzer {UserId} wurde abgelehnt.",
                storedRefreshToken.Id,
                storedRefreshToken.UserId);

            return Result<AuthenticationResponse>.Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(
            storedRefreshToken.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning(
                "Für Refresh Token {RefreshTokenId} wurde kein " +
                "Benutzer gefunden.",
                storedRefreshToken.Id);

            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Ein Refresh-Versuch für den deaktivierten Benutzer " +
                "{UserId} wurde abgelehnt.",
                user.Id);

            return Result<AuthenticationResponse>.Forbidden();
        }

        var authentication = await CreateSessionAsync(
            user,
            storedRefreshToken,
            cancellationToken);

        _logger.LogInformation(
            "Refresh Token {RefreshTokenId} von Benutzer {UserId} " +
            "wurde erfolgreich rotiert.",
            storedRefreshToken.Id,
            user.Id);

        return Result<AuthenticationResponse>.Success(
            authentication);
    }

    public async Task<Result> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Validation(
                "Der Refresh Token darf nicht leer sein.");
        }

        var storedRefreshToken =
            await _refreshTokenRepository.GetByTokenAsync(
                request.RefreshToken,
                cancellationToken);

        /*
         * Logout ist idempotent.
         * Ein unbekannter, abgelaufener oder bereits widerrufener
         * Token gilt ebenfalls als erfolgreich abgemeldet.
         */
        if (storedRefreshToken is null ||
            !storedRefreshToken.IsActive)
        {
            return Result.NoContent();
        }

        storedRefreshToken.Revoke(
            replacedByTokenHash: null,
            revokedByIp: null);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Refresh Token {RefreshTokenId} von Benutzer {UserId} " +
            "wurde durch Logout widerrufen.",
            storedRefreshToken.Id,
            storedRefreshToken.UserId);

        return Result.NoContent();
    }

    public async Task<Result> LogoutAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result.Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!user.IsActive)
        {
            return Result.Forbidden();
        }

        var activeRefreshTokens =
            await _refreshTokenRepository
                .GetActiveTokensByUserIdAsync(
                    userId,
                    cancellationToken);

        if (activeRefreshTokens.Count == 0)
        {
            return Result.NoContent();
        }

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke(
                replacedByTokenHash: null,
                revokedByIp: null);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "{TokenCount} aktive Refresh Tokens von Benutzer {UserId} " +
            "wurden widerrufen.",
            activeRefreshTokens.Count,
            userId);

        return Result.NoContent();
    }

    public async Task<Result> AssignShelterAdminAsync(
        Guid userId,
        Guid shelterId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result.Validation(
                "Es wurde keine gültige Benutzer-ID angegeben.");
        }

        if (shelterId == Guid.Empty)
        {
            return Result.Validation(
                "Es wurde keine gültige Tierheim-ID angegeben.");
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result.NotFound(
                "Benutzer wurde nicht gefunden.");
        }

        if (!user.IsActive)
        {
            return Result.Conflict(
                "Einem deaktivierten Benutzer kann kein Tierheim " +
                "zugewiesen werden.");
        }

        var shelter = await _shelterRepository.GetByIdAsync(
            shelterId,
            cancellationToken);

        if (shelter is null)
        {
            return Result.NotFound(
                "Tierheim wurde nicht gefunden.");
        }

        var alreadyShelterAdmin =
            await _userManager.IsInRoleAsync(
                user,
                Roles.ShelterAdmin);

        var previousShelterId = user.ShelterId;
        var shelterChanged =
            previousShelterId != shelterId;

        if (shelterChanged)
        {
            user.ShelterId = shelterId;

            var updateResult =
                await _userManager.UpdateAsync(
                    user);

            if (!updateResult.Succeeded)
            {
                user.ShelterId = previousShelterId;

                _logger.LogError(
                    "Das Tierheim {ShelterId} konnte Benutzer {UserId} " +
                    "nicht zugewiesen werden. Fehler: {Errors}",
                    shelterId,
                    userId,
                    FormatIdentityErrors(updateResult));

                return Result.Conflict(
                    "Die Tierheimzuweisung konnte nicht gespeichert werden.");
            }
        }

        if (!alreadyShelterAdmin)
        {
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    Roles.ShelterAdmin);

            if (!roleResult.Succeeded)
            {
                if (shelterChanged)
                {
                    user.ShelterId = previousShelterId;

                    var rollbackResult =
                        await _userManager.UpdateAsync(
                            user);

                    if (!rollbackResult.Succeeded)
                    {
                        _logger.LogCritical(
                            "Die Tierheimzuweisung von Benutzer {UserId} " +
                            "konnte nach fehlgeschlagener Rollenzuweisung " +
                            "nicht zurückgesetzt werden. Fehler: {Errors}",
                            userId,
                            FormatIdentityErrors(rollbackResult));
                    }
                }

                _logger.LogError(
                    "Die Rolle {Role} konnte Benutzer {UserId} " +
                    "nicht zugewiesen werden. Fehler: {Errors}",
                    Roles.ShelterAdmin,
                    userId,
                    FormatIdentityErrors(roleResult));

                return Result.Conflict(
                    "Die ShelterAdmin-Rolle konnte nicht zugewiesen werden.");
            }
        }

        await RevokeActiveRefreshTokensAsync(
            userId,
            cancellationToken);

        _logger.LogInformation(
            "Benutzer {UserId} wurde dem Tierheim {ShelterId} " +
            "als ShelterAdmin zugewiesen.",
            userId,
            shelterId);

        return Result.NoContent();
    }

    public async Task<Result> RemoveShelterAccessAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result.Validation(
                "Es wurde keine gültige Benutzer-ID angegeben.");
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result.NotFound(
                "Benutzer wurde nicht gefunden.");
        }

        var wasShelterAdmin =
            await _userManager.IsInRoleAsync(
                user,
                Roles.ShelterAdmin);

        var previousShelterId = user.ShelterId;

        /*
         * Die Operation ist idempotent.
         * Fehlen Rolle und ShelterId bereits, ist der gewünschte
         * Zustand schon erreicht.
         */
        if (wasShelterAdmin)
        {
            var removeRoleResult =
                await _userManager.RemoveFromRoleAsync(
                    user,
                    Roles.ShelterAdmin);

            if (!removeRoleResult.Succeeded)
            {
                _logger.LogError(
                    "Die Rolle {Role} konnte Benutzer {UserId} " +
                    "nicht entzogen werden. Fehler: {Errors}",
                    Roles.ShelterAdmin,
                    userId,
                    FormatIdentityErrors(removeRoleResult));

                return Result.Conflict(
                    "Die ShelterAdmin-Rolle konnte nicht entfernt werden.");
            }
        }

        if (previousShelterId.HasValue)
        {
            user.ShelterId = null;

            var updateResult =
                await _userManager.UpdateAsync(
                    user);

            if (!updateResult.Succeeded)
            {
                user.ShelterId = previousShelterId;

                if (wasShelterAdmin)
                {
                    var rollbackRoleResult =
                        await _userManager.AddToRoleAsync(
                            user,
                            Roles.ShelterAdmin);

                    if (!rollbackRoleResult.Succeeded)
                    {
                        _logger.LogCritical(
                            "Die Rolle {Role} von Benutzer {UserId} " +
                            "konnte nach fehlgeschlagener Entfernung " +
                            "der ShelterId nicht wiederhergestellt werden. " +
                            "Fehler: {Errors}",
                            Roles.ShelterAdmin,
                            userId,
                            FormatIdentityErrors(rollbackRoleResult));
                    }
                }

                _logger.LogError(
                    "Die ShelterId von Benutzer {UserId} konnte nicht " +
                    "entfernt werden. Fehler: {Errors}",
                    userId,
                    FormatIdentityErrors(updateResult));

                return Result.Conflict(
                    "Der Tierheimzugriff konnte nicht vollständig " +
                    "entfernt werden.");
            }
        }

        await RevokeActiveRefreshTokensAsync(
            userId,
            cancellationToken);

        _logger.LogInformation(
            "Der Tierheimzugriff von Benutzer {UserId} wurde entfernt. " +
            "Vorherige Tierheim-ID: {PreviousShelterId}.",
            userId,
            previousShelterId);

        return Result.NoContent();
    }

    public async Task<Result> SetUserActiveStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result.Validation(
                "Es wurde keine gültige Benutzer-ID angegeben.");
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result.NotFound(
                "Benutzer wurde nicht gefunden.");
        }

        /*
         * Die Operation ist idempotent.
         * Hat der Benutzer bereits den gewünschten Status,
         * muss nichts geändert werden.
         */
        if (user.IsActive == isActive)
        {
            return Result.NoContent();
        }

        var previousStatus = user.IsActive;

        user.IsActive = isActive;

        var updateResult =
            await _userManager.UpdateAsync(
                user);

        if (!updateResult.Succeeded)
        {
            user.IsActive = previousStatus;

            _logger.LogError(
                "Der Aktivstatus von Benutzer {UserId} konnte nicht auf " +
                "{IsActive} gesetzt werden. Fehler: {Errors}",
                userId,
                isActive,
                FormatIdentityErrors(updateResult));

            return Result.Conflict(
                "Der Benutzerstatus konnte nicht gespeichert werden.");
        }

        /*
         * Beim Deaktivieren werden alle aktiven Refresh Tokens
         * widerrufen.
         */
        if (!isActive)
        {
            await RevokeActiveRefreshTokensAsync(
                userId,
                cancellationToken);

            _logger.LogInformation(
                "Benutzer {UserId} wurde deaktiviert.",
                userId);

            return Result.NoContent();
        }

        _logger.LogInformation(
            "Benutzer {UserId} wurde wieder aktiviert.",
            userId);

        return Result.NoContent();
    }

    private async Task<AuthenticationResponse> CreateSessionAsync(
        ApplicationUser user,
        RefreshToken? refreshTokenToReplace,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(
            user);

        var jwtUser = new JwtUser(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles.ToList().AsReadOnly(),
            user.ShelterId);

        var authentication =
            await _jwtService.GenerateTokenAsync(
                jwtUser);

        var newRefreshToken =
            _refreshTokenService.Create(
                user.Id,
                refreshTokenToReplace?.CreatedByIp,
                refreshTokenToReplace?.UserAgent);

        if (refreshTokenToReplace is not null)
        {
            refreshTokenToReplace.Revoke(
                newRefreshToken.RefreshToken.TokenHash,
                revokedByIp: null);
        }

        await _refreshTokenRepository.AddAsync(
            newRefreshToken.RefreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return authentication with
        {
            RefreshToken =
                newRefreshToken.PlainTextToken
        };
    }

    private async Task<int> RevokeActiveRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var activeRefreshTokens =
            await _refreshTokenRepository
                .GetActiveTokensByUserIdAsync(
                    userId,
                    cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke(
                replacedByTokenHash: null,
                revokedByIp: null);
        }

        if (activeRefreshTokens.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        return activeRefreshTokens.Count;
    }

    private static string FormatIdentityErrors(
        IdentityResult result)
    {
        return string.Join(
            Environment.NewLine,
            result.Errors.Select(
                error => error.Description));
    }
}