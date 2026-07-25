using FinanceWallet.Shared.Abstractions;
using FinanceWallet.Shared.Behaviors;
using MediatR;
using Moq;

namespace FinanceWallet.UnitTests.Behaviors;

public class TransactionBehaviorTests
{
    public record TestCommand : ICommand<string>;
    public record TestQuery : IRequest<string>;

    [Fact]
    public async Task Handle_NonCommand_SkipsTransaction()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestQuery, string>(unitOfWork.Object);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var result = await behavior.Handle(new TestQuery(), next, default);

        Assert.Equal("ok", result);
        unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Command_BeginsTransactionSavesAndCommits()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestCommand, string>(unitOfWork.Object);
        unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        RequestHandlerDelegate<string> next = () => Task.FromResult("done");

        var result = await behavior.Handle(new TestCommand(), next, default);

        Assert.Equal("done", result);
        unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandWithException_RollsBack()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestCommand, string>(unitOfWork.Object);
        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("fail");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new TestCommand(), next, default));

        unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
