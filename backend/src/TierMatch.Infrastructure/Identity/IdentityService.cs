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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IShelterRepository _shelterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IShelterRepository shelterRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _shelterRepository = shelterRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

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
            Roles.User);

        if (!roleResult.Succeeded)
        {
            var deleteResult = await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                _logger.LogError(
                    "Benutzer {UserId} konnte nach fehlgeschlagener " +
                    "Rollenzuweisung nicht gelöscht werden. Fehler: {Errors}",
                    user.Id,
                    FormatIdentityErrors(deleteResult));
            }

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

        return Result<AuthenticationResponse>.Success(authentication);
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            _logger.LogWarning(
                "Fehlgeschlagener Loginversuch für eine unbekannte E-Mail-Adresse.");

            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (!user.IsActive)
        {
            return Result<AuthenticationResponse>.Forbidden();
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return Result<AuthenticationResponse>.Forbidden();
        }

        if (!signInResult.Succeeded)
        {
            return Result<AuthenticationResponse>.Unauthorized();
        }

        user.LastLoginAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result<AuthenticationResponse>.Conflict(
                "Die Anmeldung konnte nicht vollständig verarbeitet werden.");
        }

        var authentication = await CreateSessionAsync(
            user,
            refreshTokenToReplace: null,
            cancellationToken);

        return Result<AuthenticationResponse>.Success(authentication);
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

        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (storedRefreshToken is null)
        {
            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (storedRefreshToken.IsRevoked)
        {
            await RevokeActiveRefreshTokensAsync(
                storedRefreshToken.UserId,
                cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (storedRefreshToken.IsExpired)
        {
            return Result<AuthenticationResponse>.Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(
            storedRefreshToken.UserId.ToString());

        if (user is null)
        {
            return Result<AuthenticationResponse>.Unauthorized();
        }

        if (!user.IsActive)
        {
            return Result<AuthenticationResponse>.Forbidden();
        }

        var authentication = await CreateSessionAsync(
            user,
            storedRefreshToken,
            cancellationToken);

        return Result<AuthenticationResponse>.Success(authentication);
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

        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive)
        {
            return Result.NoContent();
        }

        storedRefreshToken.Revoke(
            replacedByTokenHash: null,
            revokedByIp: null);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!user.IsActive)
        {
            return Result.Forbidden();
        }

        await RevokeActiveRefreshTokensAsync(
            userId,
            cancellationToken);

        return Result.NoContent();
    }

    public async Task<Result> AssignShelterAdminAsync(
        Guid userId,
        Guid shelterId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty || shelterId == Guid.Empty)
        {
            return Result.Validation(
                "Benutzer-ID und Tierheim-ID müssen gültig sein.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.NotFound("Benutzer wurde nicht gefunden.");
        }

        if (!user.IsActive)
        {
            return Result.Conflict(
                "Einem deaktivierten Benutzer kann kein Tierheimzugriff zugewiesen werden.");
        }

        var shelter = await _shelterRepository.GetByIdAsync(
            shelterId,
            cancellationToken);

        if (shelter is null)
        {
            return Result.NotFound("Tierheim wurde nicht gefunden.");
        }

        var alreadyInRole = await _userManager.IsInRoleAsync(
            user,
            Roles.ShelterAdmin);

        if (user.ShelterId == shelterId && alreadyInRole)
        {
            return Result.NoContent();
        }

        var previousShelterId = user.ShelterId;
        user.ShelterId = shelterId;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            user.ShelterId = previousShelterId;
            return Result.Conflict(
                "Der Tierheimzugriff konnte nicht gespeichert werden.");
        }

        if (!alreadyInRole)
        {
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                Roles.ShelterAdmin);

            if (!roleResult.Succeeded)
            {
                user.ShelterId = previousShelterId;
                await _userManager.UpdateAsync(user);

                return Result.Conflict(
                    "Die Tierheimrolle konnte nicht zugewiesen werden.");
            }
        }

        await RevokeActiveRefreshTokensAsync(
            user.Id,
            cancellationToken);

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

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.NotFound("Benutzer wurde nicht gefunden.");
        }

        var hasRole = await _userManager.IsInRoleAsync(
            user,
            Roles.ShelterAdmin);

        if (user.ShelterId is null && !hasRole)
        {
            return Result.NoContent();
        }

        var previousShelterId = user.ShelterId;
        user.ShelterId = null;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            user.ShelterId = previousShelterId;
            return Result.Conflict(
                "Der Tierheimzugriff konnte nicht entfernt werden.");
        }

        if (hasRole)
        {
            var roleResult = await _userManager.RemoveFromRoleAsync(
                user,
                Roles.ShelterAdmin);

            if (!roleResult.Succeeded)
            {
                user.ShelterId = previousShelterId;
                await _userManager.UpdateAsync(user);

                return Result.Conflict(
                    "Die Tierheimrolle konnte nicht entfernt werden.");
            }
        }

        await RevokeActiveRefreshTokensAsync(
            user.Id,
            cancellationToken);

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

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.NotFound("Benutzer wurde nicht gefunden.");
        }

        if (user.IsActive == isActive)
        {
            return Result.NoContent();
        }

        user.IsActive = isActive;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            user.IsActive = !isActive;
            return Result.Conflict(
                "Der Aktivstatus konnte nicht gespeichert werden.");
        }

        if (!isActive)
        {
            await RevokeActiveRefreshTokensAsync(
                user.Id,
                cancellationToken);
        }

        return Result.NoContent();
    }

    private async Task<AuthenticationResponse> CreateSessionAsync(
        ApplicationUser user,
        RefreshToken? refreshTokenToReplace,
        CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var jwtUser = new JwtUser(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles.ToList().AsReadOnly(),
            user.ShelterId);

        var authentication = await _jwtService.GenerateTokenAsync(jwtUser);

        var newRefreshToken = _refreshTokenService.Create(
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return authentication with
        {
            RefreshToken = newRefreshToken.PlainTextToken
        };
    }

    private async Task RevokeActiveRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var activeRefreshTokens =
            await _refreshTokenRepository.GetActiveTokensByUserIdAsync(
                userId,
                cancellationToken);

        if (activeRefreshTokens.Count == 0)
        {
            return;
        }

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke(
                replacedByTokenHash: null,
                revokedByIp: null);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string FormatIdentityErrors(
        IdentityResult result)
    {
        return string.Join(
            Environment.NewLine,
            result.Errors.Select(error => error.Description));
    }
}
