using System.Security.Claims;
using FinanceWallet.Modules.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class CurrentUserServiceTests
{
    private static (CurrentUserService service, Mock<IHttpContextAccessor> accessor) CreateService()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        return (new CurrentUserService(accessor.Object), accessor);
    }

    private static void SetUser(Mock<IHttpContextAccessor> accessor, ClaimsPrincipal? principal)
    {
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = principal! });
    }

    [Fact]
    public void UserId_ReturnsNameIdentifierClaim()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        }, "Test")));

        Assert.Equal("user-123", service.UserId);
    }

    [Fact]
    public void Email_ReturnsEmailClaim()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com")
        }, "Test")));

        Assert.Equal("user@example.com", service.Email);
    }

    [Fact]
    public void Roles_ReturnsAllRoleClaims()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        }, "Test")));

        Assert.Equal(new[] { "Admin", "User" }, service.Roles);
    }

    [Fact]
    public void IsAuthenticated_ReturnsTrue_WhenAuthenticated()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, new ClaimsPrincipal(new ClaimsIdentity("Test")));

        Assert.True(service.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenNotAuthenticated()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, new ClaimsPrincipal(new ClaimsIdentity()));

        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void NoHttpContext_ReturnsNullsAndFalse()
    {
        var (service, accessor) = CreateService();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        Assert.Null(service.UserId);
        Assert.Null(service.Email);
        Assert.Empty(service.Roles);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void NoUser_ReturnsNullsAndFalse()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, null);

        Assert.Null(service.UserId);
        Assert.Null(service.Email);
        Assert.Empty(service.Roles);
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void MissingClaims_ReturnsNullsAndEmptyRoles()
    {
        var (service, accessor) = CreateService();
        SetUser(accessor, new ClaimsPrincipal(new ClaimsIdentity("Test")));

        Assert.Null(service.UserId);
        Assert.Null(service.Email);
        Assert.Empty(service.Roles);
    }
}