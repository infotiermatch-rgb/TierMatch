using MediatR;
using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password)
    : IRequest<Result<AuthenticationResponse>>;