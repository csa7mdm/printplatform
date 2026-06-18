using System;
using System.Threading;
using System.Threading.Tasks;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Abstractions
{
    public interface INotificationService
    {
        Task<Result> SendQuoteReady(Guid customerId, Guid quoteId, CancellationToken ct = default);
        Task<Result> SendOrderConfirmed(Guid customerId, Guid orderId, CancellationToken ct = default);
        Task<Result> SendJobOffered(Guid ownerId, Guid jobAssignmentId, CancellationToken ct = default);
        Task<Result> SendQCRejected(Guid ownerId, Guid jobAssignmentId, CancellationToken ct = default);
        Task<Result> SendQCPhotosForApproval(Guid customerId, Guid jobAssignmentId, string photoUrl, CancellationToken ct = default);
        Task<Result> SendShipped(Guid customerId, Guid orderId, string trackingNumber, CancellationToken ct = default);
        Task<Result> SendPayoutProcessed(Guid ownerId, decimal amount, CancellationToken ct = default);
        Task<Result> SendOtp(string phoneNumber, string otp, CancellationToken ct = default);
        Task<Result> SendAchievementUnlocked(Guid ownerId, string achievementName, CancellationToken ct = default);
        Task<Result> SendLevelUp(Guid ownerId, int newLevel, CancellationToken ct = default);
        Task<Result> SendTierUpgrade(Guid customerId, string newTier, CancellationToken ct = default);
    }
}
