using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Infrastructure.Notifications;

/// <summary>
/// Stub WhatsApp OTP sender. Generates a 6-digit code, "sends" it (logged), and
/// stores it in-memory with a 5-minute TTL for verification. A production
/// implementation would call the WhatsApp Cloud API and persist codes in a
/// distributed cache (Redis) keyed by phone number.
/// </summary>
public sealed class WhatsAppOtpSender : IOtpSender
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private static readonly ConcurrentDictionary<string, (string Code, DateTimeOffset ExpiresAt)> Store
        = new();

    private readonly ILogger<WhatsAppOtpSender> _logger;

    public WhatsAppOtpSender(ILogger<WhatsAppOtpSender> logger) => _logger = logger;

    public Task SendAsync(string phoneNumber, Lang lang, CancellationToken ct)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        Store[phoneNumber] = (code, DateTimeOffset.UtcNow.Add(Ttl));

        // STUB: real implementation posts a WhatsApp template message here.
        _logger.LogInformation(
            "[OTP STUB] Sending WhatsApp OTP {Code} to {Phone} (lang={Lang})",
            code, phoneNumber, lang);

        return Task.CompletedTask;
    }

    public Task<bool> VerifyAsync(string phoneNumber, string code, CancellationToken ct)
    {
        if (!Store.TryGetValue(phoneNumber, out var entry))
            return Task.FromResult(false);

        if (DateTimeOffset.UtcNow > entry.ExpiresAt)
        {
            Store.TryRemove(phoneNumber, out _);
            return Task.FromResult(false);
        }

        var match = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(entry.Code),
            System.Text.Encoding.UTF8.GetBytes(code));

        if (match)
            Store.TryRemove(phoneNumber, out _);

        return Task.FromResult(match);
    }
}
