using FluentValidation;
using FluentValidation.Results;
using FinanceWallet.Shared.Behaviors;
using MediatR;
using Moq;

namespace FinanceWallet.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    public record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Enumerable.Empty<IValidator<TestRequest>>());
        RequestHandlerDelegate<string> next = () => Task.FromResult("done");

        var result = await behavior.Handle(new TestRequest(), next, default);

        Assert.Equal("done", result);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNextAndReturnsResponse()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator
            .Setup(v => v.Validate(It.IsAny<ValidationContext<TestRequest>>()))
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>([validator.Object]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("done");

        var result = await behavior.Handle(new TestRequest(), next, default);

        Assert.Equal("done", result);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator
            .Setup(v => v.Validate(It.IsAny<ValidationContext<TestRequest>>()))
            .Returns(new ValidationResult([new ValidationFailure("prop", "error")]));

        var behavior = new ValidationBehavior<TestRequest, string>([validator.Object]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("done");

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(), next, default));
    }
}
