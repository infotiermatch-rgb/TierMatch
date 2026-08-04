using TierMatch.Domain.Entities;

namespace TierMatch.Api.Tests.Domain;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesActiveToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var beforeCreation = DateTime.UtcNow;

        // Act
        var token = new RefreshToken(
            userId,
            "valid-token-hash",
            expiresAt,
            "127.0.0.1",
            "TierMatch Test Client");

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("valid-token-hash", token.TokenHash);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Equal("127.0.0.1", token.CreatedByIp);
        Assert.Equal("TierMatch Test Client", token.UserAgent);

        Assert.InRange(
            token.CreatedAt,
            beforeCreation,
            afterCreation);

        Assert.Null(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenHash);
        Assert.Null(token.RevokedByIp);

        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                Guid.Empty,
                "valid-token-hash",
                expiresAt,
                null,
                null));

        // Assert
        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTokenHash_ThrowsArgumentException(
        string tokenHash)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                userId,
                tokenHash,
                expiresAt,
                null,
                null));

        // Assert
        Assert.Equal("tokenHash", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithExpiredDate_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(
                userId,
                "valid-token-hash",
                expiresAt,
                null,
                null));

        // Assert
        Assert.Equal("expiresAt", exception.ParamName);
    }

    [Fact]
    public void Revoke_WithValidValues_RevokesToken()
    {
        // Arrange
        var token = CreateValidToken();

        // Act
        token.Revoke(
            "replacement-token-hash",
            "192.168.0.10");

        // Assert
        Assert.NotNull(token.RevokedAt);
        Assert.Equal(
            "replacement-token-hash",
            token.ReplacedByTokenHash);

        Assert.Equal(
            "192.168.0.10",
            token.RevokedByIp);

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_WithoutReplacementHash_RevokesToken()
    {
        // Arrange
        var token = CreateValidToken();

        // Act
        token.Revoke(
            replacedByTokenHash: null,
            revokedByIp: null);

        // Assert
        Assert.NotNull(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenHash);
        Assert.Null(token.RevokedByIp);
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_DoesNotOverwriteExistingValues()
    {
        // Arrange
        var token = CreateValidToken();

        token.Revoke(
            "first-replacement-hash",
            "127.0.0.1");

        var firstRevokedAt = token.RevokedAt;

        // Act
        token.Revoke(
            "second-replacement-hash",
            "192.168.0.20");

        // Assert
        Assert.Equal(firstRevokedAt, token.RevokedAt);

        Assert.Equal(
            "first-replacement-hash",
            token.ReplacedByTokenHash);

        Assert.Equal(
            "127.0.0.1",
            token.RevokedByIp);
    }

    private static RefreshToken CreateValidToken()
    {
        return new RefreshToken(
            Guid.NewGuid(),
            "valid-token-hash",
            DateTime.UtcNow.AddDays(30),
            "127.0.0.1",
            "TierMatch Test Client");
    }
}