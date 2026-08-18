using System.Security.Claims;
using FinanceWallet.Shared.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FinanceWallet.Modules.Identity.Infrastructure.Services;

public class CurrentUserService (IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
    
    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public string[] Roles =>
        User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray() ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}