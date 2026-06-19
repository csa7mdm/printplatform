using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Finance;

public sealed class Payout : BaseAggregateRoot<Guid>
{
    private readonly List<Guid> _jobAssignmentIds = [];

    private Payout() { }

    private Payout(
        Guid printerOwnerUserId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        decimal grossAmountEgp,
        decimal platformFeeEgp,
        decimal netAmountEgp,
        PayoutPaymentMethod paymentMethod,
        IEnumerable<Guid> jobAssignmentIds)
    {
        Id = Guid.NewGuid();
        PrinterOwnerUserId = printerOwnerUserId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GrossAmountEgp = decimal.Round(grossAmountEgp, 2);
        PlatformFeeEgp = decimal.Round(platformFeeEgp, 2);
        NetAmountEgp = decimal.Round(netAmountEgp, 2);
        Status = PayoutStatus.ReadyToSend;
        PaymentMethod = paymentMethod;
        CreatedAt = DateTimeOffset.UtcNow;
        _jobAssignmentIds.AddRange(jobAssignmentIds.Distinct());

        RaiseDomainEvent(new PayoutCreatedEvent(Id, PrinterOwnerUserId, NetAmountEgp, CreatedAt));
    }

    public Guid PrinterOwnerUserId { get; private set; }
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public decimal GrossAmountEgp { get; private set; }
    public decimal PlatformFeeEgp { get; private set; }
    public decimal NetAmountEgp { get; private set; }
    public PayoutStatus Status { get; private set; }
    public PayoutPaymentMethod PaymentMethod { get; private set; }
    public string? ExternalReference { get; private set; }
    public IReadOnlyList<Guid> JobAssignmentIds => _jobAssignmentIds.AsReadOnly();
    public new DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? ReconciledAt { get; private set; }

    public static Result<Payout> CreateReadyToSend(
        Guid printerOwnerUserId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        decimal grossAmountEgp,
        decimal platformFeeEgp,
        decimal netAmountEgp,
        PayoutPaymentMethod paymentMethod,
        IEnumerable<Guid> jobAssignmentIds)
    {
        if (periodEnd <= periodStart)
            return FinanceErrors.InvalidPeriod;

        if (netAmountEgp <= 0 || grossAmountEgp <= 0)
            return FinanceErrors.InvalidAmount;

        var jobs = jobAssignmentIds.Distinct().ToArray();
        if (jobs.Length == 0)
            return FinanceErrors.EmptyPayoutBatch;

        return new Payout(
            printerOwnerUserId,
            periodStart,
            periodEnd,
            grossAmountEgp,
            platformFeeEgp,
            netAmountEgp,
            paymentMethod,
            jobs);
    }

    public Result MarkSent(string externalReference)
    {
        if (Status != PayoutStatus.ReadyToSend)
            return Result.Failure(FinanceErrors.PayoutNotReady);

        Status = PayoutStatus.Sent;
        ExternalReference = externalReference;
        SentAt = DateTimeOffset.UtcNow;
        Touch();
        RaiseDomainEvent(new PayoutSentEvent(Id, PrinterOwnerUserId, NetAmountEgp, externalReference, SentAt.Value));
        return Result.Success();
    }
}
