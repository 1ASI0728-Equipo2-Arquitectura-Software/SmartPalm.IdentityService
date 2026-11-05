using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Domain.Users;
using SmartPalm.IdentityService.Infrastructure.Configuration;

namespace SmartPalm.IdentityService.Infrastructure.Security;

internal class JwtTokenIssuer(IOptions<JwtOptions> options) : ITokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public string Create(User user)
    {
        if (_options.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must contain at least 32 characters.");

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("sid", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            ]),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
