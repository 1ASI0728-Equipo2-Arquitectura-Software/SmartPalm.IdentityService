namespace SmartPalm.IdentityService.Domain.Subscriptions;

public class PaymentTransaction
{
    private PaymentTransaction() { }

    public PaymentTransaction(int userId, string planName, DateTime periodStart, DateTime periodEnd, decimal amount)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (string.IsNullOrWhiteSpace(planName)) throw new ArgumentException("PlanName cannot be empty.");
        if (periodStart >= periodEnd) throw new ArgumentException("PeriodStart must be before PeriodEnd.");
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        UserId = userId;
        PlanName = planName;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Amount = amount;
        Status = PaymentStatus.Pending;
        ProcessedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string PlanName { get; private set; } = string.Empty;
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal Amount { get; private set; }
    public string? TransactionId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    public void Complete(string transactionId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be completed.");
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction id cannot be empty.");
        TransactionId = transactionId;
        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can fail.");
        Status = PaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }
}
