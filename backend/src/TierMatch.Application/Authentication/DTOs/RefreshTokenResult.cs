using TierMatch.Domain.Entities;

namespace TierMatch.Application.Authentication.DTOs;

public sealed record RefreshTokenResult(
    string PlainTextToken,
    RefreshToken RefreshToken);