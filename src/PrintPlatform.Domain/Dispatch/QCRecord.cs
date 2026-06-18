using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Dispatch;

public class QCRecord : BaseEntity<Guid>
{
    private QCRecord() { } // For EF Core

    public QCRecord(Guid jobAssignmentId, Guid operatorUserId, OperatorDecision decision, string? notes, List<string> photoStorageKeys, string? checklistJson)
    {
        Id = Guid.NewGuid();
        JobAssignmentId = jobAssignmentId;
        OperatorUserId = operatorUserId;
        Decision = decision;
        Notes = notes;
        PhotoStorageKeys = photoStorageKeys;
        ChecklistJson = checklistJson;
    }

    public Guid JobAssignmentId { get; private set; }
    public virtual JobAssignment JobAssignment { get; private set; } = null!;
    public Guid OperatorUserId { get; private set; }
    public OperatorDecision Decision { get; private set; }
    public string? Notes { get; private set; }
    public List<string> PhotoStorageKeys { get; private set; } = [];
    public string? ChecklistJson { get; private set; }
}
