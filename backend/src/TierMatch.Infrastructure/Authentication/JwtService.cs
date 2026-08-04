using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Authorization;

namespace TierMatch.Infrastructure.Authentication;

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(
        IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    public Task<AuthenticationResponse> GenerateTokenAsync(
        JwtUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var expiresAt = DateTime.UtcNow.AddMinutes(
            _options.ExpirationMinutes);

        var claims = CreateClaims(user);

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _options.SecretKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(jwtToken);

        var response = new AuthenticationResponse(
            accessToken,
            string.Empty,
            expiresAt,
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles);

        return Task.FromResult(response);
    }

    public string GenerateRefreshToken()
    {
        Span<byte> randomBytes = stackalloc byte[64];

        RandomNumberGenerator.Fill(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    public string ComputeSha256Hash(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var valueBytes = Encoding.UTF8.GetBytes(value);
        var hashBytes = SHA256.HashData(valueBytes);

        return Convert.ToHexString(hashBytes);
    }

    private static List<Claim> CreateClaims(
        JwtUser user)
    {
        List<Claim> claims =
        [
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.GivenName,
                user.FirstName),

            new Claim(
                ClaimTypes.Surname,
                user.LastName),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        ];

        if (user.ShelterId.HasValue)
        {
            claims.Add(
                new Claim(
                    CustomClaimTypes.ShelterId,
                    user.ShelterId.Value.ToString()));
        }

        foreach (var role in user.Roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        return claims;
    }
}