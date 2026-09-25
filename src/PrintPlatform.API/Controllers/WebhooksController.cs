using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using PrintPlatform.Application.Webhooks.Commands;
using PrintPlatform.Application.Webhooks.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PrintPlatform.Application.Notifications.Commands;
using PrintPlatform.Application.Notifications.Models;

namespace PrintPlatform.API.Controllers;

[Route("api/webhooks")]
[Produces("application/json")]
public class WebhooksController : ApiControllerBase
{
    private readonly IConfiguration _configuration;

    public WebhooksController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Verifies the WhatsApp webhook subscription.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("whatsapp")]
    public IActionResult VerifyWhatsAppWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.challenge")] string challenge,
        [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        var configuredToken = _configuration["WhatsApp:VerifyToken"];
        if (mode == "subscribe" && verifyToken == configuredToken)
        {
            return Ok(challenge);
        }

        return Forbid();
    }

    /// <summary>
    /// Handles incoming notifications from the WhatsApp Business API.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("whatsapp")]
    public async Task<IActionResult> HandleWhatsAppNotification()
    {
        if (!Request.Headers.TryGetValue("X-Hub-Signature-256", out StringValues signature))
        {
            return Unauthorized("Missing signature header.");
        }

        var appSecret = _configuration["WhatsApp:AppSecret"];
        if (string.IsNullOrEmpty(appSecret))
        {
            // Log error: AppSecret not configured
            return StatusCode(500, "Internal server error: webhook secret not configured.");
        }

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), Encoding.UTF8.GetBytes(body));
        var hashString = $"sha256={Convert.ToHexString(hash).ToLower()}";

        if (hashString != signature)
        {
            return Unauthorized("Invalid signature.");
        }

        var notification = JsonSerializer.Deserialize<WhatsAppNotification>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (notification != null)
        {
            await Mediator.Send(new ProcessWhatsAppMessageCommand(notification));
        }

        return Ok();
    }
}
