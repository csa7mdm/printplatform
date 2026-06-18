namespace PrintPlatform.Domain.Identity;

/// <summary>Role of a user within the two-sided marketplace.</summary>
public enum UserRole
{
    /// <summary>End customer who places print orders.</summary>
    Customer = 0,

    /// <summary>Owner/operator of one or more 3D printers who fulfils orders.</summary>
    PrinterOwner = 1,

    /// <summary>Back-office operator handling support and dispatch.</summary>
    Operator = 2,

    /// <summary>Platform administrator with full access.</summary>
    Admin = 3,
}

/// <summary>Preferred UI / communication language.</summary>
public enum Lang
{
    Arabic = 0,
    English = 1,
}

/// <summary>
/// Certification tier for a printer owner. Drives marketplace ranking,
/// pricing eligibility and trust badges.
/// </summary>
public enum CertificationLevel
{
    /// <summary>Onboarding not yet completed / under review.</summary>
    Pending = 0,
    Basic = 1,
    Certified = 2,
    Expert = 3,
    Elite = 4,
}
