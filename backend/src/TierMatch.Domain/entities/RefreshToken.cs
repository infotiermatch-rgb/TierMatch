namespace TierMatch.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? UserAgent { get; private set; }

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked =>
        RevokedAt.HasValue;

    public bool IsActive =>
        !IsExpired && !IsRevoked;

    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? createdByIp,
        string? userAgent)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "Die Benutzer-ID darf nicht leer sein.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException(
                "Der Token-Hash darf nicht leer sein.",
                nameof(tokenHash));
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Das Ablaufdatum muss in der Zukunft liegen.",
                nameof(expiresAt));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
        UserAgent = userAgent;
    }

    public void Revoke(
        string? replacedByTokenHash,
        string? revokedByIp)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        RevokedByIp = revokedByIp;
    }
}