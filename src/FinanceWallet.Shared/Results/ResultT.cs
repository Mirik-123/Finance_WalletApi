namespace FinanceWallet.Shared.Results;

public sealed class Result<TValue> : Result
{
    public TValue? Value { get; }

    internal Result(TValue? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
