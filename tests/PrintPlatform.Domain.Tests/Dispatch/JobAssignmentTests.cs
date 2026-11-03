using FluentAssertions;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Dispatch.Events;
using System;
using System.Linq;
using Xunit;

namespace PrintPlatform.Domain.Tests.Dispatch;

public class JobAssignmentTests
{
    private JobAssignment CreateOfferedJob()
    {
        var result = JobAssignment.Create(
            orderItemId: Guid.NewGuid(),
            printerId: Guid.NewGuid(),
            printerOwnerUserId: Guid.NewGuid(),
            payoutAmount: 10m,
            operatorNotes: "Test notes"
        );

        // Clear domain events that happen during creation
        result.Value.PopDomainEvents();
        return result.Value;
    }

    [Fact]
    public void Reject_WhenStatusIsOffered_ShouldSucceedAndRaiseEvent()
    {
        // Arrange
        var job = CreateOfferedJob();

        // Act
        var result = job.Reject();

        // Assert
        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobAssignmentStatus.Rejected);
        job.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        var domainEvents = job.DomainEvents;
        domainEvents.Should().HaveCount(1);

        var rejectedEvent = domainEvents.First() as JobRejectedEvent;
        rejectedEvent.Should().NotBeNull();
        rejectedEvent!.JobAssignmentId.Should().Be(job.Id);
        rejectedEvent.PrinterOwnerUserId.Should().Be(job.PrinterOwnerUserId);
        rejectedEvent.OrderItemId.Should().Be(job.OrderItemId);
        rejectedEvent.AutoRejected.Should().BeFalse(); // default value
    }

    [Fact]
    public void Reject_WithAutoRejectedTrue_ShouldRaiseEventWithAutoRejectedFlag()
    {
        // Arrange
        var job = CreateOfferedJob();

        // Act
        var result = job.Reject(autoRejected: true);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var rejectedEvent = job.DomainEvents.OfType<JobRejectedEvent>().FirstOrDefault();
        rejectedEvent.Should().NotBeNull();
        rejectedEvent!.AutoRejected.Should().BeTrue();
    }

    [Theory]
    [InlineData(JobAssignmentStatus.Accepted)]
    [InlineData(JobAssignmentStatus.Rejected)]
    [InlineData(JobAssignmentStatus.Printing)]
    [InlineData(JobAssignmentStatus.QCPending)]
    [InlineData(JobAssignmentStatus.QCApproved)]
    [InlineData(JobAssignmentStatus.QCRejected)]
    [InlineData(JobAssignmentStatus.Ready)]
    [InlineData(JobAssignmentStatus.Shipped)]
    [InlineData(JobAssignmentStatus.Delivered)]
    [InlineData(JobAssignmentStatus.Failed)]
    public void Reject_WhenStatusIsNotOffered_ShouldFail(JobAssignmentStatus invalidStatus)
    {
        // Arrange
        var job = CreateOfferedJob();

        // We need to use reflection to set the private setter of Status for testing other states
        // A better approach would be to use valid state transitions if they were all supported,
        // but given some states like 'Failed' might not have a clear transition path without other setup,
        // setting the property via reflection is the most robust way to test the guard clause.
        var statusProperty = typeof(JobAssignment).GetProperty(nameof(JobAssignment.Status));
        statusProperty!.SetValue(job, invalidStatus);

        // Act
        var result = job.Reject();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DispatchErrors.NotOffered);
        job.DomainEvents.Should().BeEmpty();
    }
}