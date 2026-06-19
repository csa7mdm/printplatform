using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Finance;

public static class FinanceErrors
{
    public static readonly Error InvalidAmount =
        Error.Validation("Finance.InvalidAmount", "Amount must be greater than zero.");

    public static readonly Error EmptyPayoutBatch =
        Error.Validation("Finance.EmptyPayoutBatch", "There are no eligible unpaid jobs for this payout period.");

    public static readonly Error PayoutNotFound =
        Error.NotFound("Finance.PayoutNotFound", "Payout was not found.");

    public static readonly Error PayoutNotReady =
        Error.Conflict("Finance.PayoutNotReady", "Only payouts ready to send can be marked as sent.");

    public static readonly Error InvalidPeriod =
        Error.Validation("Finance.InvalidPeriod", "The payout period end must be after the period start.");
}
