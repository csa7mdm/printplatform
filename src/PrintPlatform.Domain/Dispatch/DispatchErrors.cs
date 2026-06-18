using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Dispatch;

public static class DispatchErrors
{
    public static readonly Error CannotAccept = Error.Failure(
        "Job.CannotAccept", "Job cannot be accepted unless it has been offered.");

    public static readonly Error DeadlinePassed = Error.Failure(
        "Job.DeadlinePassed", "The acceptance deadline for this job has passed.");

    public static readonly Error NotOffered = Error.Failure(
        "Job.NotOffered", "Job cannot be rejected unless it is in the Offered state.");
        
    public static readonly Error NotAccepted = Error.Failure(
        "Job.NotAccepted", "Cannot start printing unless the job has been accepted.");

    public static readonly Error NotPrinting = Error.Failure(
        "Job.NotPrinting", "Cannot complete printing unless the job is currently printing.");

    public static readonly Error NotPendingQC = Error.Failure(
        "Job.NotPendingQC", "Job must be pending QC to be approved or rejected.");

    public static readonly Error NotQCApproved = Error.Failure(
        "Job.NotQCApproved", "Job must be QC Approved before it can be shipped.");
        
    public static readonly Error NotShipped = Error.Failure(
        "Job.NotShipped", "Job cannot be delivered if it has not been shipped.");
}
