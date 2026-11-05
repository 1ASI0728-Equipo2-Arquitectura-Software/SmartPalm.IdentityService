using System.Net.Mail;
using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Model.Commands;
using SmartPalm.IdentityService.Domain.Events;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Services;
using SmartPalm.IdentityService.Domain.Subscriptions;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Application.Services;

public class IdentityCommandService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    ISubscriptionRepository subscriptions,
    IIntegrationEventWriter eventWriter,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthenticatedUserResponse> SignInAsync(
        SignInCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByUsernameAsync(command.Username.Trim(), cancellationToken);
        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");
        if (user.Status != UserStatus.Active)
            throw new UnauthorizedAccessException("User access is not active.");

        return new(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            tokenIssuer.Create(user));
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);
        if (!Enum.TryParse<UserRole>(command.Role, true, out var role))
            throw new ArgumentException(
                $"Invalid role '{command.Role}'. Valid roles: Administrator, Agronomist, PalmGrower");
        if (await users.UsernameExistsAsync(command.Username.Trim(), cancellationToken))
            throw new ArgumentException($"Username '{command.Username}' is already taken.");
        if (await users.EmailExistsAsync(command.Email.Trim(), cancellationToken))
            throw new ArgumentException($"Email '{command.Email}' is already registered.");

        var user = new User(
            command.Username,
            passwordHasher.Hash(command.Password),
            command.Email,
            command.FullName,
            role);

        return await unitOfWork.ExecuteInTransactionAsync(async innerToken =>
        {
            users.Add(user);
            await unitOfWork.SaveChangesAsync(innerToken);
            eventWriter.Enqueue(
                "smartpalm.identity.user-created.v1",
                new UserCreatedIntegrationEvent(
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FullName,
                    user.Role.ToString(),
                    DateTime.UtcNow));
            if (!string.IsNullOrWhiteSpace(command.PlanType))
            {
                if (role != UserRole.PalmGrower)
                    throw new ArgumentException("Only PalmGrower accounts can have a subscription plan.");
                if (!Enum.TryParse<PlanType>(command.PlanType, true, out var planType))
                    throw new ArgumentException($"Invalid plan type '{command.PlanType}'.");

                var subscription = new Subscription(
                    user.Id,
                    SubscriptionPlanCatalog.Get(planType),
                    DateTime.UtcNow);
                subscriptions.Add(subscription);
                await unitOfWork.SaveChangesAsync(innerToken);
                eventWriter.Enqueue(
                    "smartpalm.identity.subscription-created.v1",
                    new SubscriptionCreatedIntegrationEvent(
                        subscription.Id,
                        subscription.UserId,
                        subscription.PlanType.ToString(),
                        subscription.Status.ToString(),
                        DateTime.UtcNow));
            }
            await unitOfWork.SaveChangesAsync(innerToken);
            return ToResponse(user);
        }, cancellationToken);
    }

    private static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.FullName,
        user.Role.ToString(),
        user.Status.ToString());

    private static void Validate(CreateUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || command.Username.Trim().Length is < 3 or > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters.");
        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.");
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email cannot be empty.");
        try { _ = new MailAddress(command.Email); }
        catch (FormatException) { throw new ArgumentException("Invalid email format."); }
        if (string.IsNullOrWhiteSpace(command.FullName))
            throw new ArgumentException("Full name cannot be empty.");
    }
}
