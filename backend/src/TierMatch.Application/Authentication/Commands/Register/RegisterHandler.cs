using MediatR;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Authentication.Commands.Register;

public sealed class RegisterHandler
    : IRequestHandler<RegisterCommand, Result<AuthenticationResponse>>
{
    private readonly IIdentityService _identityService;

    public RegisterHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequest(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);

        return await _identityService.RegisterAsync(
            registerRequest,
            cancellationToken);
    }
}