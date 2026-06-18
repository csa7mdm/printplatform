namespace PrintPlatform.Domain.Orders;

/// <summary>Supported 3D model file formats accepted by the platform.</summary>
public enum FileFormat
{
    Stl = 1,
    ThreeMF = 2,
    Obj = 3,
}

/// <summary>Lifecycle state of an uploaded model file as it flows through geometry analysis.</summary>
public enum ModelFileStatus
{
    Uploaded = 0,
    AnalysisPending = 1,
    Approved = 2,
    Rejected = 3,
}

/// <summary>Print quality preset selected by the customer; drives layer height / time.</summary>
public enum QualityPreset
{
    Draft = 0,
    Standard = 1,
    Fine = 2,
    Ultra = 3,
}

/// <summary>Lifecycle state of a customer's quote request.</summary>
public enum QuoteRequestStatus
{
    PendingReview = 0,
    Quoted = 1,
    Expired = 2,
    Accepted = 3,
    Rejected = 4,
}

/// <summary>Lifecycle state of an operator-produced quote.</summary>
public enum QuoteStatus
{
    Draft = 0,
    Sent = 1,
    Accepted = 2,
    Expired = 3,
}

/// <summary>Lifecycle state of a confirmed customer order.</summary>
public enum OrderStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    InProduction = 2,
    QCPending = 3,
    ReadyToShip = 4,
    Shipped = 5,
    Delivered = 6,
    Cancelled = 7,
    Refunded = 8,
}

/// <summary>Payment methods supported in the Egyptian market.</summary>
public enum PaymentMethod
{
    COD = 0,
    InstaPay = 1,
    VodafoneCash = 2,
    Card = 3,
    BankTransfer = 4,
}

/// <summary>Settlement state of an order's payment.</summary>
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    PartiallyPaid = 2,
    Refunded = 3,
}
