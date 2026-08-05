namespace TierMatch.Application.Authentication.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(
        string recipientEmail,
        string recipientName,
        string resetToken,
        CancellationToken cancellationToken = default);
}