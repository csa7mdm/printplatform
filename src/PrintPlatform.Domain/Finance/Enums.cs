namespace PrintPlatform.Domain.Finance;

public enum LedgerEntryType
{
    CustomerPayment,
    PlatformCommission,
    OwnerPayoutBatched,
    OwnerPayoutPaid,
    Refund,
    AdjustmentCredit,
    AdjustmentDebit
}

public enum LedgerDirection
{
    In,
    Out
}

public enum PayoutStatus
{
    Calculating,
    ReadyToSend,
    Sent,
    Failed,
    Reconciled
}

public enum PayoutPaymentMethod
{
    InstaPay,
    BankTransfer
}
