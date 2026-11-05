using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Model.Queries;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Application.Services;

public class IdentityQueryService(IUserRepository users)
{
    public async Task<UserResponse?> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(query.UserId, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    public async Task<IReadOnlyCollection<UserResponse>> HandleAsync(
        GetAllUsersQuery query,
        CancellationToken cancellationToken = default) =>
        (await users.ListAsync(cancellationToken)).Select(ToResponse).ToArray();

    private static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.FullName,
        user.Role.ToString(),
        user.Status.ToString());
}
