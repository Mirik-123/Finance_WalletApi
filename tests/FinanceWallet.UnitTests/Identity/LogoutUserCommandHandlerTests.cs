using FinanceWallet.Modules.Identity.Application.Abstractions;
using FinanceWallet.Modules.Identity.Application.Handlers;
using Moq;

namespace FinanceWallet.UnitTests.Identity;

public class LogoutUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_RevokesAllTokensForUser()
    {
        var refreshTokens = new Mock<IRefreshTokenRepository>();
        var handler = new LogoutUserCommandHandler(refreshTokens.Object);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        refreshTokens.Verify(r => r.RevokeAllForUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}