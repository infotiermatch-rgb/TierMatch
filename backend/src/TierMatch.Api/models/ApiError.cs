namespace TierMatch.Api.Models;

public sealed class ApiError
{
    public int Status { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;

    // Für Validierungsfehler
    public IReadOnlyList<string>? Errors { get; init; }

    public DateTime Timestamp { get; init; }

    public string TraceId { get; init; } = string.Empty;
}