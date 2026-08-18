using FinanceWallet.Modules.Identity.Domain.Entities;

namespace FinanceWallet.Modules.Identity.Contracts;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}