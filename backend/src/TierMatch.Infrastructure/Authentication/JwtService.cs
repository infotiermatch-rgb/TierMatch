using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;

namespace TierMatch.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<AuthenticationResponse> GenerateTokenAsync(
        JwtUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(
            _options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),

            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),

            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        AuthenticationResponse response = new()
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,

            UserId = user.Id,
            Email = user.Email,

            FirstName = user.FirstName,
            LastName = user.LastName,

            Roles = user.Roles.ToList()
        };

        return Task.FromResult(response);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes);
    }
}