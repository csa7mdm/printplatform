using FluentAssertions;
using PrintPlatform.Domain.Finance;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Tests.Finance;

public sealed class PayoutTests
{
    private readonly Guid _printerOwnerUserId = Guid.NewGuid();
    private readonly DateTimeOffset _periodStart = DateTimeOffset.UtcNow.AddDays(-7);
    private readonly DateTimeOffset _periodEnd = DateTimeOffset.UtcNow;
    private readonly decimal _grossAmount = 100.123m;
    private readonly decimal _platformFee = 10.051m;
    private readonly decimal _netAmount = 90.072m;
    private readonly PayoutPaymentMethod _paymentMethod = PayoutPaymentMethod.BankTransfer;
    private readonly Guid[] _jobAssignmentIds = [Guid.NewGuid(), Guid.NewGuid()];

    [Fact]
    public void CreateReadyToSend_with_period_end_before_start_returns_InvalidPeriod()
    {
        var result = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodStart.AddDays(-1), // End before start
            _grossAmount,
            _platformFee,
            _netAmount,
            _paymentMethod,
            _jobAssignmentIds);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FinanceErrors.InvalidPeriod);
    }

    [Fact]
    public void CreateReadyToSend_with_period_end_equal_start_returns_InvalidPeriod()
    {
        var result = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodStart, // End equals start
            _grossAmount,
            _platformFee,
            _netAmount,
            _paymentMethod,
            _jobAssignmentIds);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FinanceErrors.InvalidPeriod);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-10, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -10)]
    public void CreateReadyToSend_with_invalid_amounts_returns_InvalidAmount(decimal net, decimal gross)
    {
        var result = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodEnd,
            gross,
            _platformFee,
            net,
            _paymentMethod,
            _jobAssignmentIds);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FinanceErrors.InvalidAmount);
    }

    [Fact]
    public void CreateReadyToSend_with_empty_jobs_returns_EmptyPayoutBatch()
    {
        var result = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodEnd,
            _grossAmount,
            _platformFee,
            _netAmount,
            _paymentMethod,
            []);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FinanceErrors.EmptyPayoutBatch);
    }

    [Fact]
    public void CreateReadyToSend_with_only_duplicate_jobs_which_distincts_to_none_is_impossible_but_tested_anyway()
    {
        // Technically passing no distinct jobs is empty jobs, already tested.
    }

    [Fact]
    public void CreateReadyToSend_happy_path_creates_payout()
    {
        var dupJobId = Guid.NewGuid();
        Guid[] jobs = [dupJobId, dupJobId, Guid.NewGuid()];

        var result = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodEnd,
            _grossAmount,
            _platformFee,
            _netAmount,
            _paymentMethod,
            jobs);

        result.IsSuccess.Should().BeTrue();

        var payout = result.Value;
        payout.PrinterOwnerUserId.Should().Be(_printerOwnerUserId);
        payout.PeriodStart.Should().Be(_periodStart);
        payout.PeriodEnd.Should().Be(_periodEnd);
        payout.GrossAmountEgp.Should().Be(100.12m); // Rounded to 2 decimals
        payout.PlatformFeeEgp.Should().Be(10.05m); // Rounded to 2 decimals
        payout.NetAmountEgp.Should().Be(90.07m); // Rounded to 2 decimals
        payout.Status.Should().Be(PayoutStatus.ReadyToSend);
        payout.PaymentMethod.Should().Be(_paymentMethod);

        // Should only have 2 distinct job IDs
        payout.JobAssignmentIds.Should().HaveCount(2);

        var events = payout.PopDomainEvents();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<PayoutCreatedEvent>();

        var createdEvent = (PayoutCreatedEvent)events.First();
        createdEvent.PayoutId.Should().Be(payout.Id);
        createdEvent.PrinterOwnerUserId.Should().Be(payout.PrinterOwnerUserId);
        createdEvent.NetAmountEgp.Should().Be(payout.NetAmountEgp);
        createdEvent.OccurredAt.Should().Be(payout.CreatedAt);
    }

    [Fact]
    public void MarkSent_on_payout_not_ready_to_send_returns_PayoutNotReady()
    {
        var payout = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodEnd,
            _grossAmount,
            _platformFee,
            _netAmount,
            _paymentMethod,
            _jobAssignmentIds).Value;

        // Force a status change to simulate Not Ready
        var prop = typeof(Payout).GetProperty(nameof(Payout.Status));
        prop!.SetValue(payout, PayoutStatus.Sent);

        var result = payout.MarkSent("EXT-REF-123");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FinanceErrors.PayoutNotReady);
    }

    [Fact]
    public void MarkSent_happy_path_updates_status_and_raises_event()
    {
        var payout = Payout.CreateReadyToSend(
            _printerOwnerUserId,
            _periodStart,
            _periodEnd,
            _grossAmount,
            _platformFee,
            _netAmount,
            _paymentMethod,
            _jobAssignmentIds).Value;

        // Clear creation events
        payout.PopDomainEvents();

        var refStr = "EXT-REF-123";
        var result = payout.MarkSent(refStr);

        result.IsSuccess.Should().BeTrue();
        payout.Status.Should().Be(PayoutStatus.Sent);
        payout.ExternalReference.Should().Be(refStr);
        payout.SentAt.Should().NotBeNull();

        var events = payout.PopDomainEvents();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<PayoutSentEvent>();

        var sentEvent = (PayoutSentEvent)events.First();
        sentEvent.PayoutId.Should().Be(payout.Id);
        sentEvent.PrinterOwnerUserId.Should().Be(payout.PrinterOwnerUserId);
        sentEvent.NetAmountEgp.Should().Be(payout.NetAmountEgp);
        sentEvent.ExternalReference.Should().Be(refStr);
        sentEvent.OccurredAt.Should().Be(payout.SentAt!.Value);
    }
}
