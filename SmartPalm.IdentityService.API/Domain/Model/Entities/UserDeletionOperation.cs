using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Domain.Model.Entities;

public class UserDeletionOperation
{
    private UserDeletionOperation() { }

    public UserDeletionOperation(int userId, int expectedServices)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (expectedServices <= 0) throw new ArgumentOutOfRangeException(nameof(expectedServices));
        Id = Guid.NewGuid();
        UserId = userId;
        ExpectedServices = expectedServices;
        Status = UserDeletionStatus.Pending;
        RequestedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public int? UserId { get; private set; }
    public int ExpectedServices { get; private set; }
    public int CompletedServices { get; private set; }
    public UserDeletionStatus Status { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? Error { get; private set; }

    public void MarkCompleted(int completedServices)
    {
        CompletedServices = completedServices;
        Status = UserDeletionStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        UserId = null;
    }

    public void RegisterCompletion(int completedServices) => CompletedServices = completedServices;

    public void MarkFailed(string error)
    {
        Status = UserDeletionStatus.Failed;
        Error = error[..Math.Min(error.Length, 2000)];
        CompletedAtUtc = DateTime.UtcNow;
    }
}
