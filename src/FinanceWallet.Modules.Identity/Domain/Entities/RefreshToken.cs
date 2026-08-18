using FinanceWallet.Shared.Domain;
using FinanceWallet.Shared.Exceptions;

namespace FinanceWallet.Modules.Identity.Domain.Entities;

public class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? deviceName = null,
        string? userAgent = null,
        string? ipAddress = null)
    {
        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            DeviceName = deviceName,
            UserAgent = userAgent,
            IpAddress = ipAddress
        };
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt is not null;

    public bool IsActive => !IsExpired && !IsRevoked;

    public void Revoke()
    {
        if (IsRevoked)
            throw new DomainException("Cannot replace an already revoked token.");

        RevokedAt = DateTime.UtcNow;
    }

    public void RevokeAndReplace(Guid newTokenId)
    {
        Revoke();
        ReplacedByTokenId = newTokenId;
    }
}
