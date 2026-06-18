using PrintPlatform.Domain.Dispatch.Events;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Dispatch;

public class JobAssignment : BaseAggregateRoot<Guid>
{
    private readonly List<QCRecord> _qcHistory = new();

    private JobAssignment() { } // For EF Core

    public static Result<JobAssignment> Create(Guid orderItemId, Guid printerId, Guid printerOwnerUserId, decimal payoutAmount, string? operatorNotes)
    {
        var job = new JobAssignment
        {
            Id = Guid.NewGuid(),
            OrderItemId = orderItemId,
            PrinterId = printerId,
            PrinterOwnerUserId = printerOwnerUserId,
            PayoutAmount = payoutAmount,
            OperatorNotes = operatorNotes,
            Status = JobAssignmentStatus.Offered,
            PayoutStatus = PayoutStatus.Pending,
            OfferedAt = DateTimeOffset.UtcNow,
            AcceptanceDeadline = DateTimeOffset.UtcNow.AddHours(3)
        };
        
        job.RaiseDomainEvent(new JobOfferedEvent(job.Id, job.PrinterOwnerUserId, job.OrderItemId));

        return job;
    }

    public Guid OrderItemId { get; private set; }
    public Guid PrinterId { get; private set; }
    public Guid PrinterOwnerUserId { get; private set; }
    
    public JobAssignmentStatus Status { get; private set; }
    public DateTimeOffset OfferedAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset? PrintingStartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset AcceptanceDeadline { get; private set; }
    
    public decimal PayoutAmount { get; private set; }
    public PayoutStatus PayoutStatus { get; private set; }

    public string? SlicerFileStorageKey { get; private set; }
    public string? OperatorNotes { get; private set; }
    
    public byte[] RowVersion { get; private set; } = null!;
    
    public virtual IReadOnlyList<QCRecord> QcHistory => _qcHistory.AsReadOnly();

    public Result Accept()
    {
        if (Status != JobAssignmentStatus.Offered)
            return Result.Failure(DispatchErrors.CannotAccept);

        if (DateTimeOffset.UtcNow > AcceptanceDeadline)
            return Result.Failure(DispatchErrors.DeadlinePassed);

        Status = JobAssignmentStatus.Accepted;
        AcceptedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new JobAcceptedEvent(Id, PrinterOwnerUserId));
        
        return Result.Success();
    }
    
    public Result Reject(bool autoRejected = false)
    {
        if (Status != JobAssignmentStatus.Offered)
            return Result.Failure(DispatchErrors.NotOffered);

        Status = JobAssignmentStatus.Rejected;
        CompletedAt = DateTimeOffset.UtcNow; // Or a new RejectedAt property
        RaiseDomainEvent(new JobRejectedEvent(Id, PrinterOwnerUserId, OrderItemId));
        
        return Result.Success();
    }

    public Result StartPrinting()
    {
        if (Status != JobAssignmentStatus.Accepted)
            return Result.Failure(DispatchErrors.NotAccepted);
        
        Status = JobAssignmentStatus.Printing;
        PrintingStartedAt = DateTimeOffset.UtcNow;
        
        return Result.Success();
    }
    
    public Result CompletePrinting()
    {
        if (Status != JobAssignmentStatus.Printing)
            return Result.Failure(DispatchErrors.NotPrinting);
            
        Status = JobAssignmentStatus.QCPending;
        
        return Result.Success();
    }
    
    public Result ApproveQC(Guid operatorUserId, string notes, List<string> photos, string checklist)
    {
        if (Status != JobAssignmentStatus.QCPending)
            return Result.Failure(DispatchErrors.NotPendingQC);

        var qcRecord = new QCRecord(Id, operatorUserId, OperatorDecision.Approved, notes, photos, checklist);
        _qcHistory.Add(qcRecord);
        
        Status = JobAssignmentStatus.QCApproved;
        
        RaiseDomainEvent(new QCApprovedEvent(Id, PrinterOwnerUserId));
        
        return Result.Success();
    }

    public Result RejectQC(Guid operatorUserId, string notes, List<string> photos, string checklist, bool needsReprint)
    {
        if (Status != JobAssignmentStatus.QCPending)
            return Result.Failure(DispatchErrors.NotPendingQC);

        var qcRecord = new QCRecord(Id, operatorUserId, OperatorDecision.Rejected, notes, photos, checklist);
        _qcHistory.Add(qcRecord);
        
        Status = JobAssignmentStatus.QCRejected;
        
        RaiseDomainEvent(new QCRejectedEvent(Id, PrinterOwnerUserId, needsReprint));

        return Result.Success();
    }

    public Result Ship()
    {
        if (Status != JobAssignmentStatus.QCApproved)
            return Result.Failure(DispatchErrors.NotQCApproved);

        Status = JobAssignmentStatus.Shipped;
        return Result.Success();
    }

    public Result ConfirmDelivery()
    {
        if (Status != JobAssignmentStatus.Shipped)
            return Result.Failure(DispatchErrors.NotShipped);

        Status = JobAssignmentStatus.Delivered;
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new JobDeliveredEvent(Id, PrinterOwnerUserId));
        
        return Result.Success();
    }
}
