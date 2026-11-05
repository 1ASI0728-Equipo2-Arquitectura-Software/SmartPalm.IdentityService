namespace SmartPalm.IdentityService.Infrastructure.Configuration;

public class SeedOptions
{
    public const string SectionName = "Seed";
    public bool Enabled { get; init; }
    public string AdminUsername { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminFullName { get; init; } = "SmartPalm Administrator";
}
