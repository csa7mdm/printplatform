using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Dispatch.Events;

public sealed record JobOfferedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, Guid OrderItemId) : IDomainEvent;
public sealed record JobAcceptedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId) : IDomainEvent;
public sealed record JobRejectedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, Guid OrderItemId) : IDomainEvent;
public sealed record QCApprovedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId) : IDomainEvent;
public sealed record QCRejectedEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId, bool Reprint) : IDomainEvent;
public sealed record JobDeliveredEvent(Guid JobAssignmentId, Guid PrinterOwnerUserId) : IDomainEvent;
