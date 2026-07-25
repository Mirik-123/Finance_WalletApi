using MediatR;

namespace FinanceWallet.Shared.Abstractions;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : ICommand, IRequest<TResponse>
{
}
