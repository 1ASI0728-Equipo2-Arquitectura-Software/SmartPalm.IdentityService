using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.UnitTests;

public class PaymentTransactionTests
{
    [Fact]
    public void Complete_AssignsTransactionAndCompletedStatus()
    {
        var payment = new PaymentTransaction(7, "Seed", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 149m);
        payment.Complete("local-123");
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("local-123", payment.TransactionId);
    }

    [Fact]
    public void Constructor_RejectsNonPositiveAmount()
    {
        var action = () => new PaymentTransaction(7, "Seed", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 0m);
        Assert.Throws<ArgumentException>(action);
    }
}
