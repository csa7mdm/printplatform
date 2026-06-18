using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Abstractions;

/// <summary>
/// Defines a contract for sending various transactional notifications to users,
/// primarily via WhatsApp.
/// </summary>
public interface INotificationService
{
    Task SendQuoteReady(string recipientIdentifier, string orderId, string quoteDetails);
    Task<Result> SendQuoteReady(Guid customerId, Guid quoteId, CancellationToken ct = default);
    Task SendOrderConfirmed(string recipientIdentifier, string orderId);
    Task<Result> SendOrderConfirmed(Guid customerId, Guid orderId, CancellationToken ct = default);
    Task SendJobOffered(string recipientIdentifier, string jobDetails);
    Task<Result> SendJobOffered(Guid ownerId, Guid jobAssignmentId, CancellationToken ct = default);
    Task SendQCRejected(string recipientIdentifier, string orderId, string reason);
    Task<Result> SendQCRejected(Guid ownerId, Guid jobAssignmentId, CancellationToken ct = default);
    Task SendQCPhotosForApproval(string recipientIdentifier, string orderId, IEnumerable<string> photoUrls);
    Task<Result> SendQCPhotosForApproval(Guid customerId, Guid jobAssignmentId, string photoUrl, CancellationToken ct = default);
    Task SendShipped(string recipientIdentifier, string orderId, string trackingUrl);
    Task<Result> SendShipped(Guid customerId, Guid orderId, string trackingNumber, CancellationToken ct = default);
    Task SendPayoutProcessed(string recipientIdentifier, string amount, string period);
    Task<Result> SendPayoutProcessed(Guid ownerId, decimal amount, CancellationToken ct = default);
    Task<Result> SendOtp(string recipientIdentifier, string otp, CancellationToken ct = default);
    Task SendAchievementUnlocked(string recipientIdentifier, string achievementName, string achievementDescription);
    Task<Result> SendAchievementUnlocked(Guid ownerId, string achievementName, CancellationToken ct = default);
    Task SendLevelUp(string recipientIdentifier, int newLevel);
    Task<Result> SendLevelUp(Guid ownerId, int newLevel, CancellationToken ct = default);
    Task SendTierUpgrade(string recipientIdentifier, string newTier);
    Task<Result> SendTierUpgrade(Guid customerId, string newTier, CancellationToken ct = default);
}
