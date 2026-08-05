namespace TierMatch.Infrastructure.Authentication.Options;

public sealed class PasswordResetOptions
{
    public const string SectionName =
        "PasswordReset";

    public string ResetUrl { get; init; } =
        string.Empty;
}