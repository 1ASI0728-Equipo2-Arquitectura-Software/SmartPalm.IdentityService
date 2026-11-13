using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Infrastructure.Configuration;
using SmartPalm.IdentityService.Infrastructure.Messaging;
using SmartPalm.IdentityService.Infrastructure.Payments;
using SmartPalm.IdentityService.Infrastructure.Persistence;
using SmartPalm.IdentityService.Infrastructure.Security;

namespace SmartPalm.IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:IdentityDatabase is required.");

        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserDeletionRepository, UserDeletionRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IPaymentProcessor, LocalPaymentProcessor>();
        services.AddScoped<IIntegrationEventWriter, IntegrationEventWriter>();
        services.AddScoped<IdentityCommandService>();
        services.AddScoped<IdentityQueryService>();
        services.AddScoped<ReconciliationQueryService>();
        services.AddScoped<SubscriptionCommandService>();
        services.AddScoped<SubscriptionQueryService>();
        services.AddScoped<UserDeletionCommandService>();
        services.AddScoped<UserDeletionQueryService>();
        services.AddHostedService<OutboxPublisher>();
        services.AddHostedService<UserDeletionCompletionConsumer>();
        return services;
    }
}
