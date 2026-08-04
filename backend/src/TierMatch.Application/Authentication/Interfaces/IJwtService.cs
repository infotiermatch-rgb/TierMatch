using TierMatch.Application.Authentication.DTOs;

namespace TierMatch.Application.Authentication.Interfaces;

public interface IJwtService
{
    Task<AuthenticationResponse> GenerateTokenAsync(
        JwtUser user);
}