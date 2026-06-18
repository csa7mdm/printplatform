using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Identity;

/// <summary>Catalog of domain errors for the Identity module.</summary>
public static class IdentityErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("Identity.User.NotFound", "User was not found.");

    public static readonly Error EmailAlreadyInUse =
        Error.Conflict("Identity.Email.InUse", "This email address is already registered.");

    public static readonly Error PhoneAlreadyInUse =
        Error.Conflict("Identity.Phone.InUse", "This phone number is already registered.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Identity.Login.Invalid", "Invalid email or password.");

    public static readonly Error AccountInactive =
        Error.Unauthorized("Identity.Account.Inactive", "This account has been deactivated.");

    public static readonly Error AlreadyVerified =
        Error.Conflict("Identity.Phone.AlreadyVerified", "This phone number is already verified.");

    public static readonly Error InvalidOtp =
        Error.Validation("Identity.Otp.Invalid", "The verification code is invalid or has expired.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Identity.RefreshToken.Invalid", "The refresh token is invalid.");

    public static readonly Error RefreshTokenExpired =
        Error.Unauthorized("Identity.RefreshToken.Expired", "The refresh token has expired or been revoked.");

    public static readonly Error NotAPrinterOwner =
        Error.Validation("Identity.PrinterOwner.NotApplicable", "This operation is only valid for printer owners.");

    public static readonly Error OnboardingAlreadyCompleted =
        Error.Conflict("Identity.PrinterOwner.OnboardingDone", "Onboarding has already been completed.");

    public static readonly Error ProfileNotFound =
        Error.NotFound("Identity.Profile.NotFound", "Profile was not found.");
}
