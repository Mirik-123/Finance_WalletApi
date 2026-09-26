using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Domain.Entities;
using FinanceWallet.Modules.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FinanceWallet.Modules.Identity.Infrastructure.Persistence.Repositories;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public ApplicationUserRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailValue = Email.Create(email);

        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == emailValue, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailValue = Email.Create(email);

        return await _dbContext.Users
            .AnyAsync(u => u.Email == emailValue, cancellationToken);
    }

    public async Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(ApplicationUser user)
    {
        _dbContext.Users.Update(user);
    }
}