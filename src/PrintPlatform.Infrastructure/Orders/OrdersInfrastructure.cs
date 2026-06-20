using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Orders.Quoting;
using PrintPlatform.Infrastructure.Modules;

namespace PrintPlatform.Infrastructure.Orders;

// ───────────────────────── Options ─────────────────────────
public sealed class PaymobOptions
{
    public const string SectionName = "Paymob";
    public string ApiKey { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
    public string IframeId { get; set; } = string.Empty;
    public string HmacSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
}

// ───────────────────────── Paymob gateway ─────────────────────────
/// <summary>
/// Paymob v2 flow: auth token → order registration → payment key → iframe URL.
/// Real HTTP calls; configuration comes from the "Paymob" section.
/// </summary>
public sealed class PaymobGateway : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobGateway> _logger;

    public PaymobGateway(HttpClient http, IOptions<PaymobOptions> options, ILogger<PaymobGateway> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
        if (_http.BaseAddress is null && !string.IsNullOrWhiteSpace(_options.BaseUrl))
            _http.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<InitiatePaymentResult> InitiatePaymentAsync(
        InitiatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Auth token
        var auth = await PostJsonAsync("auth/tokens", new { api_key = _options.ApiKey }, cancellationToken);
        var token = auth.GetProperty("token").GetString()!;

        // 2. Order registration
        var amountCents = (int)Math.Round(request.AmountEgp * 100m);
        var order = await PostJsonAsync("ecommerce/orders", new
        {
            auth_token = token,
            delivery_needed = false,
            amount_cents = amountCents,
            currency = "EGP",
            merchant_order_id = request.OrderId.ToString(),
            items = Array.Empty<object>(),
        }, cancellationToken);
        var paymobOrderId = order.GetProperty("id").GetRawText();

        // 3. Payment key
        var key = await PostJsonAsync("acceptance/payment_keys", new
        {
            auth_token = token,
            amount_cents = amountCents,
            expiration = 3600,
            order_id = paymobOrderId,
            currency = "EGP",
            integration_id = _options.IntegrationId,
            billing_data = new
            {
                first_name = request.CustomerFirstName,
                last_name = request.CustomerLastName,
                email = request.CustomerEmail,
                phone_number = request.CustomerPhone,
                street = request.ShippingStreet,
                city = request.ShippingCity,
                country = request.ShippingCountry,
                apartment = "NA", floor = "NA", building = "NA", shipping_method = "NA",
                postal_code = "NA", state = "NA",
            },
        }, cancellationToken);
        var paymentKey = key.GetProperty("token").GetString()!;

        var iframeUrl = $"{_options.BaseUrl}/acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";
        return new InitiatePaymentResult(paymobOrderId, paymentKey, iframeUrl);
    }

    public bool ValidateWebhookSignature(IReadOnlyDictionary<string, string> payloadObj, string hmac)
    {
        if (string.IsNullOrEmpty(_options.HmacSecret)) return false;
        // Paymob concatenates a fixed ordered subset of fields, then HMAC-SHA512.
        string[] order =
        [
            "amount_cents","created_at","currency","error_occured","has_parent_transaction","id",
            "integration_id","is_3d_secure","is_auth","is_capture","is_refunded","is_standalone_payment",
            "is_voided","order.id","owner","pending","source_data.pan","source_data.sub_type",
            "source_data.type","success",
        ];
        var concatenated = string.Concat(order.Select(k => payloadObj.TryGetValue(k, out var v) ? v : string.Empty));
        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(_options.HmacSecret));
        var computed = Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(concatenated))).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(hmac.ToLowerInvariant()));
    }

    private async Task<JsonElement> PostJsonAsync(string path, object body, CancellationToken ct)
    {
        using var resp = await _http.PostAsJsonAsync(path, body, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }
}

// ───────────────────────── Model analysis job + scheduler ─────────────────────────
/// <summary>Hangfire job that estimates geometry for an uploaded model file.</summary>
public sealed class ModelGeometryAnalysisJob
{
    private readonly IAppDbContext _db;
    private readonly ILogger<ModelGeometryAnalysisJob> _logger;

    public ModelGeometryAnalysisJob(IAppDbContext db, ILogger<ModelGeometryAnalysisJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid modelFileId, CancellationToken ct = default)
    {
        var model = await _db.ModelFiles.FirstOrDefaultAsync(m => m.Id == modelFileId, ct);
        if (model is null)
        {
            _logger.LogWarning("ModelGeometryAnalysisJob: model {ModelFileId} not found", modelFileId);
            return;
        }
        // Placeholder estimate until real STL parsing is wired (volume/weight/time).
        model.ApplyAnalysis(volumeCc: 0m, boundingBoxX: 0m, boundingBoxY: 0m, boundingBoxZ: 0m,
            weightGrams: 0m, printHours: 0m,
            notes: "Auto-analysis placeholder — STL geometry parsing pending.");
        await _db.SaveChangesAsync(ct);
    }
}

public sealed class HangfireModelAnalysisJobScheduler : IModelAnalysisJobScheduler
{
    private readonly IBackgroundJobClient _jobs;
    public HangfireModelAnalysisJobScheduler(IBackgroundJobClient jobs) => _jobs = jobs;

    public void EnqueueAnalysis(Guid modelFileId)
        => _jobs.Enqueue<ModelGeometryAnalysisJob>(j => j.ExecuteAsync(modelFileId, CancellationToken.None));
}

// ───────────────────────── Module installer ─────────────────────────
public sealed class OrdersModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuotePricingOptions>(configuration.GetSection("QuotePricing"));
        services.AddScoped<IQuotePricingCalculator, QuotePricingCalculator>();

        services.Configure<PaymobOptions>(configuration.GetSection(PaymobOptions.SectionName));
        services.AddHttpClient<IPaymentGateway, PaymobGateway>();

        services.AddScoped<ModelGeometryAnalysisJob>();
        services.AddScoped<IModelAnalysisJobScheduler, HangfireModelAnalysisJobScheduler>();
    }
}
