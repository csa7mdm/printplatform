using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PrintPlatform.Infrastructure.Finance;

internal sealed class FinanceHangfireRegistrationService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobs;
    private readonly ILogger<FinanceHangfireRegistrationService> _logger;

    public FinanceHangfireRegistrationService(
        IRecurringJobManager recurringJobs,
        ILogger<FinanceHangfireRegistrationService> logger)
    {
        _recurringJobs = recurringJobs;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _recurringJobs.AddOrUpdate<WeeklyPayoutBatchJob>(
            "finance-weekly-payouts",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 0 * * 0");

        _logger.LogInformation("Registered finance weekly payout recurring job.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
