namespace SmartPalm.IdentityService.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "smartpalm.identity";
    public string Audience { get; init; } = "smartpalm.clients";
    public string Key { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 10080;
}
