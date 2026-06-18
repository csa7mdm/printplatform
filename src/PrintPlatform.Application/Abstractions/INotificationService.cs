namespace PrintPlatform.Application.Abstractions;

/// <summary>
/// Defines a contract for sending various transactional notifications to users,
/// primarily via WhatsApp.
/// </summary>
public interface INotificationService
{
    Task SendQuoteReady(string recipientIdentifier, string orderId, string quoteDetails);
    Task SendOrderConfirmed(string recipientIdentifier, string orderId);
    Task SendJobOffered(string recipientIdentifier, string jobDetails);
    Task SendQCRejected(string recipientIdentifier, string orderId, string reason);
    Task SendQCPhotosForApproval(string recipientIdentifier, string orderId, IEnumerable<string> photoUrls);
    Task SendShipped(string recipientIdentifier, string orderId, string trackingUrl);
    Task SendPayoutProcessed(string recipientIdentifier, string amount, string period);
    Task SendOtp(string recipientIdentifier, string otp);
    Task SendAchievementUnlocked(string recipientIdentifier, string achievementName, string achievementDescription);
    Task SendLevelUp(string recipientIdentifier, int newLevel);
    Task SendTierUpgrade(string recipientIdentifier, string newTier);
}
