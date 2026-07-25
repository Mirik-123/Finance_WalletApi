using FinanceWallet.Shared.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceWallet.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    public record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_LogsBeforeAndAfter()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger.Object);
        RequestHandlerDelegate<string> next = () => Task.FromResult("done");

        var result = await behavior.Handle(new TestRequest(), next, default);

        Assert.Equal("done", result);
        logger.Verify(
            l => l.Log(
                It.Is<LogLevel>(ll => ll == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains("Processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ReturnsResponseFromNext()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger.Object);
        RequestHandlerDelegate<string> next = () => Task.FromResult("response");

        var result = await behavior.Handle(new TestRequest(), next, default);

        Assert.Equal("response", result);
    }
}
