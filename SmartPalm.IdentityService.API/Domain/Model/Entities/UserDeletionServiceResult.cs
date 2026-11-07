namespace SmartPalm.IdentityService.Domain.Model.Entities;

public class UserDeletionServiceResult
{
    private UserDeletionServiceResult() { }

    public UserDeletionServiceResult(Guid operationId, string serviceName, int deletedRecords)
    {
        OperationId = operationId;
        ServiceName = serviceName.Trim();
        DeletedRecords = deletedRecords;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public Guid OperationId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public int DeletedRecords { get; private set; }
    public DateTime CompletedAtUtc { get; private set; }
}
