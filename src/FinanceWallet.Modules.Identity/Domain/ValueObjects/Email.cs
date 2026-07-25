using System.Text.RegularExpressions;
using FinanceWallet.Shared.Domain;
using FinanceWallet.Shared.Exceptions;

namespace FinanceWallet.Modules.Identity.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = EmailRegexPattern();

    [GeneratedRegex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegexPattern();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > 254)
            throw new DomainException("Email is too long.");

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException("Email format is invalid.");

        return new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
