namespace PrintPlatform.Gamification;

/// <summary>
/// Well-known action strings passed to <c>AwardPointsAsync</c>.
/// The host may pass arbitrary custom strings; only these receive a default XP mapping.
/// </summary>
public static class GamificationActions
{
    public const string JobAccepted       = "JobAccepted";
    public const string QcApproved        = "QCApproved";
    public const string QcRejected        = "QCRejected";
    public const string OrderDelivered    = "OrderDelivered";
    public const string ProfileCompleted  = "ProfileCompleted";
    public const string FirstJobCompleted = "FirstJobCompleted";
    public const string ReviewReceived    = "ReviewReceived";
    public const string ChallengeCompleted = "ChallengeCompleted";
}
