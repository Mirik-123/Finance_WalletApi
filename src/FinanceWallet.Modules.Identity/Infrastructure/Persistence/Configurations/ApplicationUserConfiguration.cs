using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceWallet.Modules.Identity.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", "identity");

        builder.HasKey(u => u.Id);

        // Identity
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(254)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Profile
        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        // Audity
        builder.Property(u => u.CreatedAt)
            .HasPrecision(3)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasPrecision(3);

        builder.Property(u => u.IsDeleted)
            .IsRequired();

        builder.Property(u => u.DeletedAt)
            .HasPrecision(3);

        // Security
        builder.Property(u => u.AccessFailedCount)
            .IsRequired();

        builder.Property(u => u.LockoutEnd)
            .HasPrecision(3);

        builder.Property(u => u.LastLoginAt)
            .HasPrecision(3);

        builder.Property(u => u.EmailConfirmed)
            .IsRequired();

        builder.HasQueryFilter(u => !u.IsDeleted);

        // Ignores
        builder.Ignore(u => u.DomainEvents);
    }
}