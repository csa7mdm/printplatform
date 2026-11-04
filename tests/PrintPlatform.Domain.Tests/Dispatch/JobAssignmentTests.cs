using System;
using System.Linq;
using FluentAssertions;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Dispatch.Events;
using Xunit;

namespace PrintPlatform.Domain.Tests.Dispatch;

public class JobAssignmentTests
{
    private static JobAssignment CreateDefaultJobAssignment()
    {
        return JobAssignment.Create(
            orderItemId: Guid.NewGuid(),
            printerId: Guid.NewGuid(),
            printerOwnerUserId: Guid.NewGuid(),
            payoutAmount: 100m,
            operatorNotes: "Test notes").Value;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Reject_WhenStatusIsOffered_ShouldReturnSuccessAndSetState(bool autoRejected)
    {
        // Arrange
        var job = CreateDefaultJobAssignment();
        job.PopDomainEvents(); // Clear creation events

        var before = DateTimeOffset.UtcNow;

        // Act
        var result = job.Reject(autoRejected);

        // Assert
        var after = DateTimeOffset.UtcNow;

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobAssignmentStatus.Rejected);

        job.CompletedAt.Should().NotBeNull();
        job.CompletedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        var events = job.PopDomainEvents();
        events.Should().ContainSingle(e => e is JobRejectedEvent);
        var rejectedEvent = (JobRejectedEvent)events.Single(e => e is JobRejectedEvent);
        rejectedEvent.JobAssignmentId.Should().Be(job.Id);
        rejectedEvent.PrinterOwnerUserId.Should().Be(job.PrinterOwnerUserId);
        rejectedEvent.OrderItemId.Should().Be(job.OrderItemId);
    }

    [Fact]
    public void Reject_WhenStatusIsNotOffered_ShouldReturnFailure()
    {
        // Arrange
        var job = CreateDefaultJobAssignment();
        job.Accept(); // Transitions to Accepted state
        job.PopDomainEvents(); // Clear events from creation and accept

        var completedAtBefore = job.CompletedAt;

        // Act
        var result = job.Reject();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DispatchErrors.NotOffered);

        job.Status.Should().Be(JobAssignmentStatus.Accepted);
        job.CompletedAt.Should().Be(completedAtBefore);

        job.PopDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldReturnFailure()
    {
        // Arrange
        var job = CreateDefaultJobAssignment();
        job.Reject(); // Initial rejection
        job.PopDomainEvents(); // Clear events from initial rejection

        var completedAtBefore = job.CompletedAt;

        // Act
        var result = job.Reject();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DispatchErrors.NotOffered); // Because status is now Rejected, not Offered

        job.Status.Should().Be(JobAssignmentStatus.Rejected);
        job.CompletedAt.Should().Be(completedAtBefore);

        job.PopDomainEvents().Should().BeEmpty();
    }
}
