using Microsoft.EntityFrameworkCore;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Infrastructure.Persistence;

internal class UserRepository(IdentityDbContext context) : IUserRepository
{
    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Username == username, cancellationToken);

    public async Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Users.AsNoTracking().OrderBy(user => user.Id).ToArrayAsync(cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Username == username, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default) =>
        context.Users.CountAsync(user => user.Role == UserRole.Administrator && user.Status == UserStatus.Active, cancellationToken);

    public void Add(User user) => context.Users.Add(user);
    public void Remove(User user) => context.Users.Remove(user);
}
