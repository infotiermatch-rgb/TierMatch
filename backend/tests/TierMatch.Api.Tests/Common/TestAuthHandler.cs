using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TierMatch.Application.Authorization;

namespace TierMatch.Api.Tests.Common;

public sealed class TestAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Test";

    public const string UserIdHeader = "X-Test-User-Id";
    public const string RolesHeader = "X-Test-Roles";
    public const string ShelterIdHeader = "X-Test-Shelter-Id";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                UserIdHeader,
                out var userIdHeader))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        if (!Guid.TryParse(
                userIdHeader.ToString(),
                out var userId))
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Die Test-Benutzer-ID ist ungültig."));
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Name,
                $"TestUser-{userId}")
        };

        if (Request.Headers.TryGetValue(
                RolesHeader,
                out var rolesHeader))
        {
            var roles = rolesHeader
                .ToString()
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }
        }

        if (Request.Headers.TryGetValue(
                ShelterIdHeader,
                out var shelterIdHeader) &&
            Guid.TryParse(
                shelterIdHeader.ToString(),
                out var shelterId))
        {
            claims.Add(
                new Claim(
                    CustomClaimTypes.ShelterId,
                    shelterId.ToString()));
        }

        var identity = new ClaimsIdentity(
            claims,
            AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            AuthenticationScheme);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}