using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Infrastructure.Authentication.Options;

namespace TierMatch.Infrastructure.Email;

/// <summary>
/// Entwicklungsimplementierung des E-Mail-Dienstes.
///
/// Es wird noch keine echte E-Mail versendet. Der Link zur
/// Passwortzurücksetzung wird im Anwendungsprotokoll ausgegeben.
///
/// Diese Implementierung muss vor einer produktiven Veröffentlichung
/// durch einen echten E-Mail-Anbieter ersetzt werden.
/// </summary>
public sealed class DevelopmentEmailService
    : IEmailService
{
    private readonly PasswordResetOptions _options;
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(
        IOptions<PasswordResetOptions> options,
        ILogger<DevelopmentEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(
                _options.ResetUrl))
        {
            throw new InvalidOperationException(
                "Die Konfiguration PasswordReset:ResetUrl fehlt.");
        }

        if (!Uri.TryCreate(
                _options.ResetUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "PasswordReset:ResetUrl enthält keine gültige URL.");
        }

        var encodedEmail =
            Uri.EscapeDataString(
                recipientEmail);

        var encodedToken =
            Uri.EscapeDataString(
                resetToken);

        var separator =
            _options.ResetUrl.Contains(
                '?',
                StringComparison.Ordinal)
                ? "&"
                : "?";

        var resetLink =
            $"{_options.ResetUrl}{separator}" +
            $"email={encodedEmail}&token={encodedToken}";

        _logger.LogInformation(
            """
            Entwicklungs-E-Mail zur Passwortzurücksetzung:

            Empfänger: {RecipientEmail}
            Name: {RecipientName}
            Link: {ResetLink}
            """,
            recipientEmail,
            recipientName,
            resetLink);

        return Task.CompletedTask;
    }
}