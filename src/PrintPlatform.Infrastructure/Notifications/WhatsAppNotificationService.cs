using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Shared;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PrintPlatform.Infrastructure.Notifications;

public class WhatsAppNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppNotificationService> _logger;
    private readonly WhatsAppOptions _options;

    public WhatsAppNotificationService(HttpClient httpClient, IOptions<WhatsAppOptions> options, ILogger<WhatsAppNotificationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri($"{_options.ApiUrl}{_options.PhoneNumberId}/");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);
    }

    private Task SendTemplateMessage(string recipient, string templateName, object? payload = null)
    {
        _logger.LogInformation("Sending WhatsApp template '{TemplateName}' to {Recipient}", templateName, recipient);
        // Fire-and-forget as per spec
        #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        _httpClient.PostAsJsonAsync("messages", new
        {
            messaging_product = "whatsapp",
            to = recipient,
            type = "template",
            template = new
            {
                name = _options.TemplateNames.GetValueOrDefault(templateName, templateName),
                language = new { code = "en_US" } // or from user profile
            }
        }).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                _logger.LogError(t.Exception, "Failed to send WhatsApp message '{TemplateName}' to {Recipient}", templateName, recipient);
            }
            else if (!t.Result.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send WhatsApp message '{TemplateName}' to {Recipient}. Status: {StatusCode}, Body: {Body}",
                    templateName, recipient, t.Result.StatusCode, t.Result.Content.ReadAsStringAsync().Result);
            }
        });
        #pragma warning restore CS4014
        return Task.CompletedTask;
    }

    public Task SendQuoteReady(string recipient, string orderId, string quoteDetails)
        => SendTemplateMessage(recipient, "SendQuoteReady");

    public async Task<Result> SendQuoteReady(Guid customerId, Guid quoteId, CancellationToken ct = default)
    {
        await SendQuoteReady(customerId.ToString(), quoteId.ToString(), string.Empty);
        return Result.Success();
    }

    public Task SendOrderConfirmed(string recipient, string orderId)
        => SendTemplateMessage(recipient, "SendOrderConfirmed");

    public async Task<Result> SendOrderConfirmed(Guid customerId, Guid orderId, CancellationToken ct = default)
    {
        await SendOrderConfirmed(customerId.ToString(), orderId.ToString());
        return Result.Success();
    }

    public Task SendJobOffered(string recipient, string jobDetails)
        => SendTemplateMessage(recipient, "SendJobOffered");

    public async Task<Result> SendJobOffered(Guid ownerId, Guid jobAssignmentId, CancellationToken ct = default)
    {
        await SendJobOffered(ownerId.ToString(), jobAssignmentId.ToString());
        return Result.Success();
    }

    public Task SendQCRejected(string recipient, string orderId, string reason)
        => SendTemplateMessage(recipient, "SendQCRejected");

    public async Task<Result> SendQCRejected(Guid ownerId, Guid jobAssignmentId, CancellationToken ct = default)
    {
        await SendQCRejected(ownerId.ToString(), jobAssignmentId.ToString(), string.Empty);
        return Result.Success();
    }

    public Task SendQCPhotosForApproval(string recipient, string orderId, IEnumerable<string> photoUrls)
        => SendTemplateMessage(recipient, "SendQCPhotosForApproval");

    public async Task<Result> SendQCPhotosForApproval(Guid customerId, Guid jobAssignmentId, string photoUrl, CancellationToken ct = default)
    {
        await SendQCPhotosForApproval(customerId.ToString(), jobAssignmentId.ToString(), new[] { photoUrl });
        return Result.Success();
    }

    public Task SendShipped(string recipient, string orderId, string trackingUrl)
        => SendTemplateMessage(recipient, "SendShipped");

    public async Task<Result> SendShipped(Guid customerId, Guid orderId, string trackingNumber, CancellationToken ct = default)
    {
        await SendShipped(customerId.ToString(), orderId.ToString(), trackingNumber);
        return Result.Success();
    }

    public Task SendPayoutProcessed(string recipient, string amount, string period)
        => SendTemplateMessage(recipient, "SendPayoutProcessed");

    public async Task<Result> SendPayoutProcessed(Guid ownerId, decimal amount, CancellationToken ct = default)
    {
        await SendPayoutProcessed(ownerId.ToString(), amount.ToString("0.##"), string.Empty);
        return Result.Success();
    }

    public async Task<Result> SendOtp(string recipient, string otp, CancellationToken ct = default)
    {
        await SendTemplateMessage(recipient, "SendOtp");
        return Result.Success();
    }

    public Task SendOtp(string recipient, string otp)
        => SendTemplateMessage(recipient, "SendOtp");

    public Task SendAchievementUnlocked(string recipient, string achievementName, string achievementDescription)
        => SendTemplateMessage(recipient, "SendAchievementUnlocked");

    public async Task<Result> SendAchievementUnlocked(Guid ownerId, string achievementName, CancellationToken ct = default)
    {
        await SendAchievementUnlocked(ownerId.ToString(), achievementName, achievementName);
        return Result.Success();
    }

    public Task SendLevelUp(string recipient, int newLevel)
        => SendTemplateMessage(recipient, "SendLevelUp");

    public async Task<Result> SendLevelUp(Guid ownerId, int newLevel, CancellationToken ct = default)
    {
        await SendLevelUp(ownerId.ToString(), newLevel);
        return Result.Success();
    }

    public Task SendTierUpgrade(string recipient, string newTier)
        => SendTemplateMessage(recipient, "SendTierUpgrade");

    public async Task<Result> SendTierUpgrade(Guid customerId, string newTier, CancellationToken ct = default)
    {
        await SendTierUpgrade(customerId.ToString(), newTier);
        return Result.Success();
    }
}
