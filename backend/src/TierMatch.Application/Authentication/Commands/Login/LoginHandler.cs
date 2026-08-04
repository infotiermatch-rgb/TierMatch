using MediatR;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Authentication.Commands.Login;

public sealed class LoginHandler
    : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest(
            request.Email,
            request.Password);

        return await _identityService.LoginAsync(
            loginRequest,
            cancellationToken);
    }
}