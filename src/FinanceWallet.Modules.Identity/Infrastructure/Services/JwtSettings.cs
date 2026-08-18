using System.ComponentModel.DataAnnotations;

namespace FinanceWallet.Modules.Identity.Infrastructure.Services;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [MinLength(32, ErrorMessage = "JwtSettings:Secret must be at least 32 characters.")]
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpirationInMinutes { get; set; } = 60;
}