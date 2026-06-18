using Hangfire;

namespace PrintPlatform.Infrastructure.BackgroundJobs;

public static class HangfireJobRegistrar
{
    public static void RegisterJobs()
    {
        // Spec: weekly-payouts "0 0 * * 0"
        RecurringJob.AddOrUpdate(
            "weekly-payouts",
            () => Console.WriteLine("Initiating weekly payouts..."), // Placeholder for IMediator.Send(new InitiateWeeklyPayoutsCommand())
            "0 0 * * 0");

        // Spec: acceptance-check "*/5 * * * *"
        RecurringJob.AddOrUpdate(
            "acceptance-check",
            () => Console.WriteLine("Checking for expired job offers..."), // Placeholder for IMediator.Send(new AcceptanceDeadlineCheckCommand())
            "*/5 * * * *");

        // Spec: loyalty-expiry "0 2 * * *"
        RecurringJob.AddOrUpdate(
            "loyalty-expiry",
            () => Console.WriteLine("Processing loyalty point expiry..."), // Placeholder for IMediator.Send(new ProcessLoyaltyExpiryCommand())
            "0 2 * * *");
    }
}
