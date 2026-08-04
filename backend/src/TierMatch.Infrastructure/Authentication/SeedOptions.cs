namespace TierMatch.Infrastructure.Authentication;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; init; } = string.Empty;

    public string AdminPassword { get; init; } = string.Empty;

    public string FirstName { get; init; } = "System";

    public string LastName { get; init; } = "Administrator";
}