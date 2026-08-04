using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Common.Results;

using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Authentication;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        // Prüfen, ob die E-Mail bereits existiert
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return Result<AuthenticationResponse>.Validation(
                "Ein Benutzer mit dieser E-Mail existiert bereits.");
        }

        // Standardrolle automatisch erstellen
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole<Guid>("User"));

            if (!roleResult.Succeeded)
            {
                _logger.LogError(
                    "Die Standardrolle 'User' konnte nicht erstellt werden.");

                return Result<AuthenticationResponse>.Conflict(
                    "Die Standardrolle konnte nicht erstellt werden.");
            }
        }

        // Benutzer erstellen
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,

            FirstName = request.FirstName,
            LastName = request.LastName,

            EmailConfirmed = true,
            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            return Result<AuthenticationResponse>.Validation(
                string.Join(
                    Environment.NewLine,
                    createResult.Errors.Select(x => x.Description)));
        }

        // Standardrolle vergeben
        var addRoleResult = await _userManager.AddToRoleAsync(
            user,
            "User");

        if (!addRoleResult.Succeeded)
        {
            return Result<AuthenticationResponse>.Conflict(
                "Die Benutzerrolle konnte nicht vergeben werden.");
        }

        // Rollen laden
        var roles = await _userManager.GetRolesAsync(user);

        // DTO für JWT erstellen
        var jwtUser = new JwtUser(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles.ToList().AsReadOnly());

        // JWT erzeugen
        var authentication =
            await _jwtService.GenerateTokenAsync(jwtUser);

        _logger.LogInformation(
            "Neuer Benutzer registriert: {Email}",
            user.Email);

        return Result<AuthenticationResponse>.Success(authentication);
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByEmailAsync(request.Email);

    if (user is null)
    {
        return Result<AuthenticationResponse>.Unauthorized();
    }

    if (!user.IsActive)
    {
        return Result<AuthenticationResponse>.Forbidden();
    }

    var passwordValid = await _signInManager.CheckPasswordSignInAsync(
        user,
        request.Password,
        lockoutOnFailure: true);

    if (!passwordValid.Succeeded)
    {
        return Result<AuthenticationResponse>.Unauthorized();
    }

    user.LastLoginAt = DateTime.UtcNow;

    await _userManager.UpdateAsync(user);

    var roles = await _userManager.GetRolesAsync(user);

    var jwtUser = new JwtUser(
        user.Id,
        user.Email!,
        user.FirstName,
        user.LastName,
        roles.ToList().AsReadOnly());

    var authentication =
        await _jwtService.GenerateTokenAsync(jwtUser);

    _logger.LogInformation(
        "Benutzer '{Email}' erfolgreich angemeldet.",
        user.Email);

    return Result<AuthenticationResponse>.Success(authentication);
}
}