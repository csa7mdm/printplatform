using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintPlatform.Application.Abstractions;
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

    public Task SendOrderConfirmed(string recipient, string orderId)
        => SendTemplateMessage(recipient, "SendOrderConfirmed");

    public Task SendJobOffered(string recipient, string jobDetails)
        => SendTemplateMessage(recipient, "SendJobOffered");

    public Task SendQCRejected(string recipient, string orderId, string reason)
        => SendTemplateMessage(recipient, "SendQCRejected");

    public Task SendQCPhotosForApproval(string recipient, string orderId, IEnumerable<string> photoUrls)
        => SendTemplateMessage(recipient, "SendQCPhotosForApproval");

    public Task SendShipped(string recipient, string orderId, string trackingUrl)
        => SendTemplateMessage(recipient, "SendShipped");

    public Task SendPayoutProcessed(string recipient, string amount, string period)
        => SendTemplateMessage(recipient, "SendPayoutProcessed");

    public Task SendOtp(string recipient, string otp)
        => SendTemplateMessage(recipient, "SendOtp");

    public Task SendAchievementUnlocked(string recipient, string achievementName, string achievementDescription)
        => SendTemplateMessage(recipient, "SendAchievementUnlocked");

    public Task SendLevelUp(string recipient, int newLevel)
        => SendTemplateMessage(recipient, "SendLevelUp");

    public Task SendTierUpgrade(string recipient, string newTier)
        => SendTemplateMessage(recipient, "SendTierUpgrade");
}
