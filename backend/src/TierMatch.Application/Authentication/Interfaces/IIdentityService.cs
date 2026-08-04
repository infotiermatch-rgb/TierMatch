using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Authentication.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}