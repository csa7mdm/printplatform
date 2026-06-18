using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Dispatch.Events;

public sealed record JobOfferedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, Guid OrderItemId, DateTimeOffset OccurredAt) : IDomainEvent
{
    public JobOfferedEvent(Guid jobAssignmentId, Guid printerOwnerUserId, Guid orderItemId)
        : this(jobAssignmentId, printerOwnerUserId, orderItemId, DateTimeOffset.UtcNow) { }
}

public sealed record JobAcceptedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, DateTimeOffset OccurredAt) : IDomainEvent
{
    public JobAcceptedEvent(Guid jobAssignmentId, Guid printerOwnerUserId)
        : this(jobAssignmentId, printerOwnerUserId, DateTimeOffset.UtcNow) { }
}

public sealed record JobRejectedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, Guid OrderItemId, DateTimeOffset OccurredAt) : IDomainEvent
{
    public JobRejectedEvent(Guid jobAssignmentId, Guid printerOwnerUserId, Guid orderItemId)
        : this(jobAssignmentId, printerOwnerUserId, orderItemId, DateTimeOffset.UtcNow) { }
}

public sealed record QCApprovedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, DateTimeOffset OccurredAt) : IDomainEvent
{
    public QCApprovedEvent(Guid jobAssignmentId, Guid printerOwnerUserId)
        : this(jobAssignmentId, printerOwnerUserId, DateTimeOffset.UtcNow) { }
}

public sealed record QCRejectedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, bool Reprint, DateTimeOffset OccurredAt) : IDomainEvent
{
    public QCRejectedEvent(Guid jobAssignmentId, Guid printerOwnerUserId, bool reprint)
        : this(jobAssignmentId, printerOwnerUserId, reprint, DateTimeOffset.UtcNow) { }
}

public sealed record JobDeliveredEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, DateTimeOffset OccurredAt) : IDomainEvent
{
    public JobDeliveredEvent(Guid jobAssignmentId, Guid printerOwnerUserId)
        : this(jobAssignmentId, printerOwnerUserId, DateTimeOffset.UtcNow) { }
}
