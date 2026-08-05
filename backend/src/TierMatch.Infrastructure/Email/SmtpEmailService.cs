using System.Net;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Infrastructure.Authentication.Options;
using TierMatch.Infrastructure.Email.Options;

namespace TierMatch.Infrastructure.Email;

/// <summary>
/// Versendet TierMatch-E-Mails über einen konfigurierten
/// SMTP-Server.
/// </summary>
public sealed class SmtpEmailService
    : IEmailService
{
    private readonly SmtpOptions _smtpOptions;
    private readonly PasswordResetOptions _passwordResetOptions;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<SmtpOptions> smtpOptions,
        IOptions<PasswordResetOptions> passwordResetOptions,
        ILogger<SmtpEmailService> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _passwordResetOptions = passwordResetOptions.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            ValidateConfiguration();

            var resetLink =
                BuildPasswordResetLink(
                    recipientEmail,
                    resetToken);

            var message =
                CreatePasswordResetMessage(
                    recipientEmail,
                    recipientName,
                    resetLink);

            var socketOptions =
                ParseSocketOptions(
                    _smtpOptions.Security);

            using var smtpClient =
                new SmtpClient
                {
                    Timeout =
                        checked(
                            _smtpOptions.TimeoutSeconds *
                            1000)
                };

            await smtpClient.ConnectAsync(
                _smtpOptions.Host,
                _smtpOptions.Port,
                socketOptions,
                cancellationToken);

            await smtpClient.AuthenticateAsync(
                _smtpOptions.Username,
                _smtpOptions.Password,
                cancellationToken);

            await smtpClient.SendAsync(
                message,
                cancellationToken);

            await smtpClient.DisconnectAsync(
                quit: true,
                cancellationToken);

            _logger.LogInformation(
                "Die Passwort-Reset-E-Mail für {RecipientEmail} " +
                "wurde erfolgreich über SMTP versendet.",
                recipientEmail);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            /*
             * Der Fehler wird absichtlich nicht an den Controller
             * weitergegeben.
             *
             * Der Forgot-Password-Endpunkt soll unabhängig davon,
             * ob eine Adresse existiert oder eine E-Mail erfolgreich
             * versendet wurde, dieselbe Antwort liefern.
             */
            _logger.LogError(
                exception,
                "Die Passwort-Reset-E-Mail für {RecipientEmail} " +
                "konnte nicht versendet werden.",
                recipientEmail);
        }
    }

    private MimeMessage CreatePasswordResetMessage(
        string recipientEmail,
        string recipientName,
        string resetLink) 
    {
        var safeRecipientName =
            WebUtility.HtmlEncode(
                recipientName);

        var safeResetLink =
            WebUtility.HtmlEncode(
                resetLink);

        var message =
            new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _smtpOptions.FromName,
                _smtpOptions.FromEmail));

        message.To.Add(
            new MailboxAddress(
                recipientName,
                recipientEmail));

        message.Subject =
            "TierMatch – Passwort zurücksetzen";

        var bodyBuilder =
            new BodyBuilder
            {
                TextBody =
                    $"""
                    Hallo {recipientName},

                    für dein TierMatch-Konto wurde eine
                    Passwortzurücksetzung angefordert.

                    Öffne den folgenden Link:

                    {resetLink}

                    Falls du die Zurücksetzung nicht angefordert hast,
                    kannst du diese E-Mail ignorieren.

                    Dein TierMatch-Team
                    """,

                HtmlBody =
                    $$"""
                    <!DOCTYPE html>
                    <html lang="de">
                    <head>
                        <meta charset="utf-8">
                        <meta name="viewport"
                              content="width=device-width, initial-scale=1">
                        <title>TierMatch – Passwort zurücksetzen</title>
                    </head>
                    <body style="
                        margin: 0;
                        padding: 24px;
                        background-color: #f4f4f4;
                        font-family: Arial, Helvetica, sans-serif;
                        color: #222222;">

                        <div style="
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 32px;
                            background-color: #ffffff;
                            border-radius: 12px;">

                            <h1 style="
                                margin-top: 0;
                                font-size: 24px;">
                                Passwort zurücksetzen
                            </h1>

                            <p>
                                Hallo {{safeRecipientName}},
                            </p>

                            <p>
                                für dein TierMatch-Konto wurde eine
                                Passwortzurücksetzung angefordert.
                            </p>

                            <p style="
                                margin-top: 32px;
                                margin-bottom: 32px;">
                                <a href="{{safeResetLink}}"
                                   style="
                                       display: inline-block;
                                       padding: 14px 22px;
                                       border-radius: 8px;
                                       background-color: #222222;
                                       color: #ffffff;
                                       text-decoration: none;
                                       font-weight: bold;">
                                    Neues Passwort festlegen
                                </a>
                            </p>

                            <p>
                                Falls der Button nicht funktioniert,
                                kopiere diesen Link in deinen Browser:
                            </p>

                            <p style="
                                overflow-wrap: anywhere;">
                                {{safeResetLink}}
                            </p>

                            <p>
                                Falls du die Zurücksetzung nicht
                                angefordert hast, kannst du diese
                                E-Mail ignorieren.
                            </p>

                            <p style="
                                margin-top: 32px;">
                                Dein TierMatch-Team
                            </p>
                        </div>
                    </body>
                    </html>
                    """
            };

        message.Body =
            bodyBuilder.ToMessageBody();

        return message;
    }

    private string BuildPasswordResetLink(
        string recipientEmail,
        string resetToken)
    {
        var encodedEmail =
            Uri.EscapeDataString(
                recipientEmail);

        var encodedToken =
            Uri.EscapeDataString(
                resetToken);

        var separator =
            _passwordResetOptions.ResetUrl.Contains(
                '?',
                StringComparison.Ordinal)
                ? "&"
                : "?";

        return
            $"{_passwordResetOptions.ResetUrl}" +
            $"{separator}" +
            $"email={encodedEmail}" +
            $"&token={encodedToken}";
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(
                _smtpOptions.Host))
        {
            throw new InvalidOperationException(
                "Die Konfiguration Smtp:Host fehlt.");
        }

        if (_smtpOptions.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "Smtp:Port muss zwischen 1 und 65535 liegen.");
        }

        if (string.IsNullOrWhiteSpace(
                _smtpOptions.Username))
        {
            throw new InvalidOperationException(
                "Die Konfiguration Smtp:Username fehlt.");
        }

        if (string.IsNullOrWhiteSpace(
                _smtpOptions.Password))
        {
            throw new InvalidOperationException(
                "Die Konfiguration Smtp:Password fehlt.");
        }

        if (string.IsNullOrWhiteSpace(
                _smtpOptions.FromEmail))
        {
            throw new InvalidOperationException(
                "Die Konfiguration Smtp:FromEmail fehlt.");
        }

        if (_smtpOptions.TimeoutSeconds is < 1 or > 300)
        {
            throw new InvalidOperationException(
                "Smtp:TimeoutSeconds muss zwischen 1 und 300 liegen.");
        }

        if (string.IsNullOrWhiteSpace(
                _passwordResetOptions.ResetUrl))
        {
            throw new InvalidOperationException(
                "Die Konfiguration PasswordReset:ResetUrl fehlt.");
        }

        if (!Uri.TryCreate(
                _passwordResetOptions.ResetUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "PasswordReset:ResetUrl enthält keine gültige URL.");
        }
    }

    private static SecureSocketOptions
        ParseSocketOptions(
            string configuredValue)
    {
        if (Enum.TryParse<SecureSocketOptions>(
                configuredValue,
                ignoreCase: true,
                out var socketOptions))
        {
            return socketOptions;
        }

        throw new InvalidOperationException(
            "Smtp:Security enthält keinen gültigen Wert. " +
            "Erlaubt sind Auto, None, StartTls, " +
            "StartTlsWhenAvailable und SslOnConnect.");
    }
}