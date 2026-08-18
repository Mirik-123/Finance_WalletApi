using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Infrastructure.Persistence;
using FinanceWallet.Modules.Identity.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class EntityConfigurationsTests
{
    private static IMutableEntityType Configure<TEntity>(IEntityTypeConfiguration<TEntity> configuration)
        where TEntity : class
    {
        var modelBuilder = new ModelBuilder();
        configuration.Configure(modelBuilder.Entity<TEntity>());
        return modelBuilder.Model.FindEntityType(typeof(TEntity))!;
    }

    private static IModel BuildIdentityModel()
    {
        var mediator = new Mock<IMediator>();
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-{Guid.NewGuid()}")
            .Options;

        using var context = new IdentityDbContext(options, mediator.Object);
        return context.Model;
    }

    [Fact]
    public void ApplicationUserConfiguration_MapsTableAndKey()
    {
        var entity = Configure(new ApplicationUserConfiguration());

        Assert.Equal("Users", entity.GetTableName());
        Assert.Equal("identity", entity.GetSchema());
        Assert.Equal(nameof(ApplicationUser.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
    }

    [Fact]
    public void ApplicationUserConfiguration_ConfiguresEmailProperty()
    {
        var entity = Configure(new ApplicationUserConfiguration());
        var email = entity.FindProperty(nameof(ApplicationUser.Email))!;

        Assert.False(email.IsNullable);
        Assert.Equal(254, email.GetMaxLength());
        Assert.False(email.IsUnicode());
        Assert.NotNull(email.GetValueConverter());
    }

    [Fact]
    public void ApplicationUserConfiguration_ConfiguresStringAndEnumProperties()
    {
        var entity = Configure(new ApplicationUserConfiguration());

        var passwordHash = entity.FindProperty(nameof(ApplicationUser.PasswordHash))!;
        Assert.False(passwordHash.IsNullable);
        Assert.Equal(256, passwordHash.GetMaxLength());

        var status = entity.FindProperty(nameof(ApplicationUser.Status))!;
        Assert.Equal(20, status.GetMaxLength());

        var role = entity.FindProperty(nameof(ApplicationUser.Role))!;
        Assert.Equal(20, role.GetMaxLength());

        var firstName = entity.FindProperty(nameof(ApplicationUser.FirstName))!;
        Assert.Equal(100, firstName.GetMaxLength());

        var emailConfirmed = entity.FindProperty(nameof(ApplicationUser.EmailConfirmed))!;
        Assert.False(emailConfirmed.IsNullable);
    }

    [Fact]
    public void IdentityModel_ConvertsEnumsAndEmailToStoreTypes()
    {
        var model = BuildIdentityModel();
        var entity = model.FindEntityType(typeof(ApplicationUser))!;

        Assert.NotNull(entity.FindProperty(nameof(ApplicationUser.Email))!.GetValueConverter());
        Assert.NotNull(entity.FindProperty(nameof(ApplicationUser.Status))!.FindTypeMapping()!.Converter);
        Assert.NotNull(entity.FindProperty(nameof(ApplicationUser.Role))!.FindTypeMapping()!.Converter);
    }

    [Fact]
    public void ApplicationUserConfiguration_ConfiguresTimestampsWithPrecision()
    {
        var entity = Configure(new ApplicationUserConfiguration());

        Assert.Equal(3, entity.FindProperty(nameof(ApplicationUser.CreatedAt))!.GetPrecision());
        Assert.Equal(3, entity.FindProperty(nameof(ApplicationUser.UpdatedAt))!.GetPrecision());
        Assert.Equal(3, entity.FindProperty(nameof(ApplicationUser.DeletedAt))!.GetPrecision());
        Assert.Equal(3, entity.FindProperty(nameof(ApplicationUser.LockoutEnd))!.GetPrecision());
        Assert.Equal(3, entity.FindProperty(nameof(ApplicationUser.LastLoginAt))!.GetPrecision());
    }

    [Fact]
    public void ApplicationUserConfiguration_AddsUniqueEmailIndexWithSoftDeleteFilter()
    {
        var entity = Configure(new ApplicationUserConfiguration());

        var index = entity.GetIndexes().Single(i => i.Properties.Single().Name == nameof(ApplicationUser.Email));

        Assert.True(index.IsUnique);
        Assert.Equal("[IsDeleted] = 0", index.GetFilter());
    }

    [Fact]
    public void ApplicationUserConfiguration_AppliesSoftDeleteQueryFilter()
    {
        var entity = Configure(new ApplicationUserConfiguration());

        Assert.NotNull(entity.GetQueryFilter());
    }

    [Fact]
    public void ApplicationUserConfiguration_IgnoresDomainEvents()
    {
        var entity = Configure(new ApplicationUserConfiguration());

        Assert.Contains(nameof(ApplicationUser.DomainEvents), entity.GetIgnoredMembers());
    }

    [Fact]
    public void RefreshTokenConfiguration_MapsTableAndKey()
    {
        var entity = Configure(new RefreshTokenConfiguration());

        Assert.Equal("RefreshTokens", entity.GetTableName());
        Assert.Equal("identity", entity.GetSchema());
        Assert.Equal(nameof(RefreshToken.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
    }

    [Fact]
    public void RefreshTokenConfiguration_ConfiguresRequiredProperties()
    {
        var entity = Configure(new RefreshTokenConfiguration());

        var userId = entity.FindProperty(nameof(RefreshToken.UserId))!;
        Assert.False(userId.IsNullable);

        var tokenHash = entity.FindProperty(nameof(RefreshToken.TokenHash))!;
        Assert.False(tokenHash.IsNullable);
        Assert.Equal(128, tokenHash.GetMaxLength());

        var expiresAt = entity.FindProperty(nameof(RefreshToken.ExpiresAt))!;
        Assert.False(expiresAt.IsNullable);
        Assert.Equal(3, expiresAt.GetPrecision());
    }

    [Fact]
    public void RefreshTokenConfiguration_ConfiguresOptionalProperties()
    {
        var entity = Configure(new RefreshTokenConfiguration());

        Assert.Equal(3, entity.FindProperty(nameof(RefreshToken.RevokedAt))!.GetPrecision());
        Assert.Equal(3, entity.FindProperty(nameof(RefreshToken.CreatedAt))!.GetPrecision());
        Assert.Equal(100, entity.FindProperty(nameof(RefreshToken.DeviceName))!.GetMaxLength());
        Assert.Equal(500, entity.FindProperty(nameof(RefreshToken.UserAgent))!.GetMaxLength());
        Assert.Equal(45, entity.FindProperty(nameof(RefreshToken.IpAddress))!.GetMaxLength());
    }

    [Fact]
    public void RefreshTokenConfiguration_AddsIndexes()
    {
        var entity = Configure(new RefreshTokenConfiguration());

        var userIdIndex = entity.GetIndexes().Single(i => i.Properties.Single().Name == nameof(RefreshToken.UserId));
        Assert.False(userIdIndex.IsUnique);

        var tokenHashIndex = entity.GetIndexes().Single(i => i.Properties.Single().Name == nameof(RefreshToken.TokenHash));
        Assert.True(tokenHashIndex.IsUnique);
    }

    [Fact]
    public void RefreshTokenConfiguration_IgnoresComputedProperties()
    {
        var entity = Configure(new RefreshTokenConfiguration());

        Assert.Contains(nameof(RefreshToken.IsExpired), entity.GetIgnoredMembers());
        Assert.Contains(nameof(RefreshToken.IsRevoked), entity.GetIgnoredMembers());
        Assert.Contains(nameof(RefreshToken.IsActive), entity.GetIgnoredMembers());
    }

    [Fact]
    public void AllConfigurations_ApplyWithoutError()
    {
        var model = BuildIdentityModel();

        var entityTypes = model.GetEntityTypes().Select(e => e.ClrType).ToHashSet();
        Assert.Contains(typeof(ApplicationUser), entityTypes);
        Assert.Contains(typeof(RefreshToken), entityTypes);
    }
}