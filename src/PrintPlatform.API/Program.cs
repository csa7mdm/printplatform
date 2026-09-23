using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using PrintPlatform.API.Middleware;
using PrintPlatform.Application;
using PrintPlatform.Gamification;
using PrintPlatform.Infrastructure;
using PrintPlatform.Infrastructure.BackgroundJobs;
using PrintPlatform.Infrastructure.Data;
using PrintPlatform.Loyalty;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

// ---------------------------------------------------------------------------
// Bootstrap Serilog early so startup errors are captured.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Serilog — reads full config from appsettings.json → "Serilog" section.
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithProperty("Application", "PrintPlatform"));

    // -----------------------------------------------------------------------
    // Application layers
    // -----------------------------------------------------------------------
    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddGamification(builder.Configuration)
        .AddLoyalty(builder.Configuration);

    // Current-user accessor (HTTP-bound) — required by Identity handlers.
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<
        PrintPlatform.Application.Identity.Abstractions.ICurrentUserAccessor,
        PrintPlatform.API.Identity.HttpCurrentUserAccessor>();

    // -----------------------------------------------------------------------
    // Authentication — JWT Bearer
    // -----------------------------------------------------------------------
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSection["Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtSection["Issuer"],
                ValidAudience            = jwtSection["Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew                = TimeSpan.FromSeconds(30),
            };
        });

    builder.Services.AddAuthorization();

    // -----------------------------------------------------------------------
    // OpenAPI / Scalar — JWT scheme wired in
    // -----------------------------------------------------------------------
    builder.Services.AddOpenApi(opts =>
    {
        opts.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title   = "PrintPlatform API";
            doc.Info.Version = "v1";
            return Task.CompletedTask;
        });
    });

    // -----------------------------------------------------------------------
    // CORS — origins from config; defaults to localhost for development.
    // -----------------------------------------------------------------------
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:3000", "http://localhost:5173"];

    builder.Services.AddCors(opts =>
        opts.AddPolicy("Frontend", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));

    // -----------------------------------------------------------------------
    // Hangfire — background jobs stored in PostgreSQL
    // -----------------------------------------------------------------------
    var hangfireConn = builder.Configuration.GetConnectionString("HangfireConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("HangfireConnection is not configured.");

    builder.Services.AddHangfire(cfg =>
        cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(hangfireConn)));

    // The background server eagerly connects to storage; skip it under integration
    // tests (they don't exercise background workers and shouldn't depend on it).
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.AddHangfireServer(opts =>
        {
            opts.WorkerCount  = Environment.ProcessorCount * 2;
            opts.Queues       = ["critical", "default", "low"];
        });
    }

    // -----------------------------------------------------------------------
    // Controllers, Health Checks, Exception Handling
    // -----------------------------------------------------------------------
    builder.Services.AddControllers();

    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "database",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"])
        .AddS3(s3 =>
            {
                var storage = builder.Configuration.GetSection("Storage");
                s3.Credentials = new Amazon.Runtime.BasicAWSCredentials(
                    storage["AccessKey"] ?? string.Empty,
                    storage["SecretKey"] ?? string.Empty);
                s3.BucketName = storage["BucketName"] ?? string.Empty;
                s3.S3Config = new Amazon.S3.AmazonS3Config
                {
                    ServiceURL = storage["Endpoint"],
                    ForcePathStyle = true,
                    UseHttp = !bool.Parse(storage["UseHttps"] ?? "true")
                };
            },
            name: "storage",
            failureStatus: HealthStatus.Degraded,
            tags: ["ready"]);

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();


    // -----------------------------------------------------------------------
    // Build & configure middleware pipeline
    // -----------------------------------------------------------------------
    var app = builder.Build();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(opts =>
        opts.EnrichDiagnosticContext = (diag, ctx) =>
        {
            diag.Set("RequestHost",   ctx.Request.Host.Value ?? string.Empty);
            diag.Set("RequestScheme", ctx.Request.Scheme);
        });

    if (app.Environment.IsDevelopment())
    {
        // Scalar UI at /scalar/v1
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();
    app.UseCors("Frontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health/live", new()
    {
        Predicate = _ => false,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    app.MapHealthChecks("/health/ready", new()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Hangfire dashboard + recurring jobs both touch storage at startup;
    // skip under integration tests.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthFilter()]
        });

        // Register recurring jobs
        HangfireJobRegistrar.RegisterJobs();
    }

    // -- Seed database ------------------------------------------------------
    // Skipped under integration tests, where the test harness owns schema + seeding.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

// Exposed so integration tests can reference the entry-point assembly's Program type.
public partial class Program { }
