using SmartPalm.IdentityService.Application.Abstractions;

namespace SmartPalm.IdentityService.Infrastructure.Security;

internal class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, 12);
    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
