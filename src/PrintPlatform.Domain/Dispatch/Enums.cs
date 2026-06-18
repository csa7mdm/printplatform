namespace PrintPlatform.Domain.Dispatch;

public enum JobAssignmentStatus
{
    Offered = 1,
    Accepted = 2,
    Rejected = 3,
    Printing = 4,
    QCPending = 5,
    QCApproved = 6,
    QCRejected = 7,
    Ready = 8,
    Shipped = 9,
    Delivered = 10,
    Failed = 11
}

public enum PayoutStatus
{
    Pending,
    Batched,
    Paid
}

public enum OperatorDecision
{
    Approved,
    Rejected
}
