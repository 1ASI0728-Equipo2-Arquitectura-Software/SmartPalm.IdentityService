using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Application.Abstractions;

public interface ITokenIssuer
{
    string Create(User user);
}
