using PrintPlatform.Domain.Dispatch;

namespace PrintPlatform.Application.Dispatch;

public record JobAssignmentDto(
    Guid Id,
    Guid OrderItemId,
    Guid PrinterId,
    Guid PrinterOwnerUserId,
    JobAssignmentStatus Status,
    DateTimeOffset OfferedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? PrintingStartedAt,
    DateTimeOffset? CompletedAt,
    decimal PayoutAmount,
    PayoutStatus PayoutStatus,
    string? OperatorNotes
);

public record QCRecordDto(
    Guid Id,
    Guid JobAssignmentId,
    Guid OperatorUserId,
    OperatorDecision Decision,
    string? Notes,
    List<string> PhotoStorageKeys,
    string? ChecklistJson,
    DateTimeOffset CreatedAt
);
