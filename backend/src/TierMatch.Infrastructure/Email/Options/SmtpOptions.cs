namespace TierMatch.Infrastructure.Email.Options;

public sealed class SmtpOptions
{
    public const string SectionName =
        "Smtp";

    public bool Enabled { get; init; }

    public string Host { get; init; } =
        string.Empty;

    public int Port { get; init; } =
        587;

    /// <summary>
    /// Unterstützte Werte:
    /// Auto, None, StartTls, StartTlsWhenAvailable,
    /// SslOnConnect.
    /// </summary>
    public string Security { get; init; } =
        "StartTls";

    public string Username { get; init; } =
        string.Empty;

    public string Password { get; init; } =
        string.Empty;

    public string FromEmail { get; init; } =
        string.Empty;

    public string FromName { get; init; } =
        "TierMatch";

    public int TimeoutSeconds { get; init; } =
        30;
}