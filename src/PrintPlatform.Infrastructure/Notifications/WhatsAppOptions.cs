namespace PrintPlatform.Infrastructure.Notifications;

public class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    public string ApiUrl { get; set; } = "https://graph.facebook.com/v18.0/";
    public string PhoneNumberId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public Dictionary<string, string> TemplateNames { get; set; } = [];
}
