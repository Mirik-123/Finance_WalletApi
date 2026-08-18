using FinanceWallet.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceWallet.Modules.Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "identity");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .HasPrecision(3)
            .IsRequired();

        builder.Property(rt => rt.RevokedAt)
            .HasPrecision(3);

        builder.Property(rt => rt.CreatedAt)
            .HasPrecision(3)
            .IsRequired();

        builder.Property(rt => rt.ReplacedByTokenId);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(100);

        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500);

        builder.Property(rt => rt.IpAddress)
            // Max length of IPv6 string representation
            .HasMaxLength(45);

        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsActive);
    }
}