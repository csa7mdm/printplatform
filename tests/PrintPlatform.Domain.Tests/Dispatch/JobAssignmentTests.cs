using FluentAssertions;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Dispatch.Events;

namespace PrintPlatform.Domain.Tests.Dispatch;

public class JobAssignmentTests
{
    private readonly Guid _validOrderItemId = Guid.NewGuid();
    private readonly Guid _validPrinterId = Guid.NewGuid();
    private readonly Guid _validPrinterOwnerUserId = Guid.NewGuid();
    private readonly decimal _validPayoutAmount = 100.50m;
    private readonly string _validOperatorNotes = "Valid notes";

    [Fact]
    public void Create_WithValidInputs_CreatesJobAssignmentSuccessfully()
    {
        // Act
        var result = JobAssignment.Create(
            _validOrderItemId,
            _validPrinterId,
            _validPrinterOwnerUserId,
            _validPayoutAmount,
            _validOperatorNotes);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var job = result.Value;
        job.Should().NotBeNull();
        job.Id.Should().NotBeEmpty();
        job.OrderItemId.Should().Be(_validOrderItemId);
        job.PrinterId.Should().Be(_validPrinterId);
        job.PrinterOwnerUserId.Should().Be(_validPrinterOwnerUserId);
        job.PayoutAmount.Should().Be(_validPayoutAmount);
        job.OperatorNotes.Should().Be(_validOperatorNotes);

        job.Status.Should().Be(JobAssignmentStatus.Offered);
        job.PayoutStatus.Should().Be(PayoutStatus.Pending);
        job.OfferedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        job.AcceptanceDeadline.Should().BeCloseTo(DateTimeOffset.UtcNow.AddHours(3), TimeSpan.FromSeconds(5));

        var domainEvents = job.DomainEvents;
        domainEvents.Should().ContainSingle();
        var domainEvent = domainEvents.First();
        domainEvent.Should().BeOfType<JobOfferedEvent>();

        var jobOfferedEvent = (JobOfferedEvent)domainEvent;
        jobOfferedEvent.JobAssignmentId.Should().Be(job.Id);
        jobOfferedEvent.PrinterOwnerUserId.Should().Be(_validPrinterOwnerUserId);
        jobOfferedEvent.OrderItemId.Should().Be(_validOrderItemId);
    }

    [Fact]
    public void Create_WithEmptyOrderItemId_ReturnsFailure()
    {
        // Act
        var result = JobAssignment.Create(
            Guid.Empty,
            _validPrinterId,
            _validPrinterOwnerUserId,
            _validPayoutAmount,
            _validOperatorNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DispatchErrors.InvalidOrderItemId);
    }

    [Fact]
    public void Create_WithEmptyPrinterId_ReturnsFailure()
    {
        // Act
        var result = JobAssignment.Create(
            _validOrderItemId,
            Guid.Empty,
            _validPrinterOwnerUserId,
            _validPayoutAmount,
            _validOperatorNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DispatchErrors.InvalidPrinterId);
    }

    [Fact]
    public void Create_WithEmptyPrinterOwnerUserId_ReturnsFailure()
    {
        // Act
        var result = JobAssignment.Create(
            _validOrderItemId,
            _validPrinterId,
            Guid.Empty,
            _validPayoutAmount,
            _validOperatorNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DispatchErrors.InvalidPrinterOwnerUserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.5)]
    public void Create_WithInvalidPayoutAmount_ReturnsFailure(decimal invalidPayoutAmount)
    {
        // Act
        var result = JobAssignment.Create(
            _validOrderItemId,
            _validPrinterId,
            _validPrinterOwnerUserId,
            invalidPayoutAmount,
            _validOperatorNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DispatchErrors.InvalidPayoutAmount);
    }

    [Fact]
    public void Create_WithExcessivelyLongNotes_ReturnsFailure()
    {
        // Arrange
        var longNotes = new string('a', 1001);

        // Act
        var result = JobAssignment.Create(
            _validOrderItemId,
            _validPrinterId,
            _validPrinterOwnerUserId,
            _validPayoutAmount,
            longNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DispatchErrors.NotesTooLong);
    }
}
