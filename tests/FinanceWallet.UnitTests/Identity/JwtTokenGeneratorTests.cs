using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceWallet.Modules.Identity.Contracts;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.Enums;
using FinanceWallet.Modules.Identity.Infrastructure.Services;
using FinanceWallet.Shared.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class JwtTokenGeneratorTests
{
    private const string Secret = "test-secret-key-that-is-at-least-32-characters-long";
    private const string Issuer = "FinanceWallet";
    private const string Audience = "FinanceWallet.Clients";
    private const int ExpirationInMinutes = 60;

    private static readonly DateTime FixedNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static ApplicationUser CreateUser(string? firstName = null, string? lastName = null) =>
        ApplicationUser.Create("user@example.com", "hashed-password", firstName, lastName);

    private static JwtTokenGenerator CreateGenerator()
    {
        var dateTime = new Mock<IDateTimeProvider>();
        dateTime.Setup(d => d.UtcNow).Returns(FixedNow);

        var settings = Options.Create(new JwtSettings
        {
            Secret = Secret,
            Issuer = Issuer,
            Audience = Audience,
            ExpirationInMinutes = ExpirationInMinutes
        });

        return new JwtTokenGenerator(settings, dateTime.Object);
    }

    private static JwtSecurityToken Parse(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void GenerateToken_ReturnsReadableToken()
    {
        var token = CreateGenerator().GenerateToken(CreateUser());

        var jwt = Parse(token);

        Assert.NotNull(jwt);
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Equal(Audience, jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Aud).Value);
    }

    [Fact]
    public void GenerateToken_SetsExpirationBasedOnSettings()
    {
        var token = CreateGenerator().GenerateToken(CreateUser());

        var jwt = Parse(token);

        Assert.Equal(FixedNow, jwt.ValidFrom);
        Assert.Equal(FixedNow.AddMinutes(ExpirationInMinutes), jwt.ValidTo);
    }

    [Fact]
    public void GenerateToken_IncludesIdentityClaims()
    {
        var user = CreateUser();
        var token = CreateGenerator().GenerateToken(user);

        var jwt = Parse(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(user.Email.Value, jwt.Claims.Single(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(user.SecurityStamp, jwt.Claims.Single(c => c.Type == "security_stamp").Value);
        Assert.NotNull(jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
    }

    [Fact]
    public void GenerateToken_OmitsOptionalClaims_WhenNotProvided()
    {
        var user = CreateUser();
        var token = CreateGenerator().GenerateToken(user);

        var jwt = Parse(token);

        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.GivenName);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.Surname);
    }

    [Fact]
    public void GenerateToken_IncludesNames_WhenProvided()
    {
        var user = CreateUser("John", "Doe");
        var token = CreateGenerator().GenerateToken(user);

        var jwt = Parse(token);

        Assert.Equal("John", jwt.Claims.Single(c => c.Type == ClaimTypes.GivenName).Value);
        Assert.Equal("Doe", jwt.Claims.Single(c => c.Type == ClaimTypes.Surname).Value);
    }

    [Fact]
    public void GenerateToken_IncludesRole_WhenNotDefault()
    {
        var user = CreateUser();
        user.ChangeRole(RoleNames.Admin);
        var token = CreateGenerator().GenerateToken(user);

        var jwt = Parse(token);

        Assert.Equal(RoleNames.Admin.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_IncludesRole_WhenDefaultRole()
    {
        var user = CreateUser();
        var token = CreateGenerator().GenerateToken(user);

        var jwt = Parse(token);

        Assert.Equal(RoleNames.User.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GenerateToken_ProducesUniqueTokens()
    {
        var generator = CreateGenerator();
        var user = CreateUser();

        var first = generator.GenerateToken(user);
        var second = generator.GenerateToken(user);

        Assert.NotEqual(first, second);
    }
}