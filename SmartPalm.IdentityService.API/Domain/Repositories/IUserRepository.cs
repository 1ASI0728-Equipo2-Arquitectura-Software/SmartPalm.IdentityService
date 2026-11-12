using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default);
    void Remove(User user);
    void Add(User user);
}
