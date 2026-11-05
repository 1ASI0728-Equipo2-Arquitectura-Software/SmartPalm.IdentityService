using Microsoft.EntityFrameworkCore;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Subscriptions;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Infrastructure.Persistence;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentTransaction> Payments => Set<PaymentTransaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Id).ValueGeneratedOnAdd();
            builder.Property(user => user.Username).HasMaxLength(50).IsRequired();
            builder.Property(user => user.PasswordHash).HasMaxLength(200).IsRequired();
            builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
            builder.Property(user => user.FullName).HasMaxLength(160).IsRequired();
            builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.HasIndex(user => user.Username).IsUnique();
            builder.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Subscription>(builder =>
        {
            builder.ToTable("subscriptions");
            builder.HasKey(subscription => subscription.Id);
            builder.Property(subscription => subscription.Id).ValueGeneratedOnAdd();
            builder.Property(subscription => subscription.PlanType).HasConversion<string>().HasMaxLength(32);
            builder.Property(subscription => subscription.PlanName).HasMaxLength(100).IsRequired();
            builder.Property(subscription => subscription.Price).HasPrecision(18, 2);
            builder.Property(subscription => subscription.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(subscription => subscription.BillingCycle).HasConversion<string>().HasMaxLength(16);
            builder.HasIndex(subscription => new { subscription.UserId, subscription.CreatedAt });
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(subscription => subscription.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentTransaction>(builder =>
        {
            builder.ToTable("payment_transactions");
            builder.HasKey(payment => payment.Id);
            builder.Property(payment => payment.Id).ValueGeneratedOnAdd();
            builder.Property(payment => payment.PlanName).HasMaxLength(100).IsRequired();
            builder.Property(payment => payment.Amount).HasPrecision(18, 2);
            builder.Property(payment => payment.TransactionId).HasMaxLength(100);
            builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32);
            builder.HasIndex(payment => new { payment.UserId, payment.ProcessedAt });
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(payment => payment.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(200).IsRequired();
            builder.Property(message => message.Content).HasColumnType("jsonb").IsRequired();
            builder.Property(message => message.Error).HasMaxLength(2000);
            builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc });
        });
    }

    public new Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        base.SaveChangesAsync(cancellationToken);

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
