using System.Security.Claims;

using TierMatch.Application.Authorization;
using TierMatch.Application.Interfaces;

namespace TierMatch.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        ParseGuidClaim(ClaimTypes.NameIdentifier);

    public Guid? ShelterId =>
        ParseGuidClaim(CustomClaimTypes.ShelterId);

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return User?.IsInRole(role) == true;
    }

    private Guid? ParseGuidClaim(
        string claimType)
    {
        var value = User?
            .FindFirst(claimType)?
            .Value;

        return Guid.TryParse(value, out var id)
            ? id
            : null;
    }
}