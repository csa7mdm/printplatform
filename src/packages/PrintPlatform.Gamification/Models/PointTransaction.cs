namespace PrintPlatform.Gamification.Models;

/// <summary>
/// Append-only ledger entry for every XP event.
/// Rows are never updated or deleted — they form a full audit trail.
/// </summary>
public sealed class PointTransaction
{
    /// <summary>Surrogate primary key.</summary>
    public long Id { get; set; }

    /// <summary>FK to <see cref="GamificationPlayer.Id"/>.</summary>
    public long PlayerId { get; set; }

    /// <summary>XP amount awarded (always positive for rewards; negative not used).</summary>
    public int Points { get; set; }

    /// <summary>
    /// Action that triggered the award.
    /// Matches one of <see cref="GamificationActions"/> constants or a custom host string.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Optional JSON bag of context supplied by the host (e.g. <c>{"jobId":"abc123"}</c>).
    /// Stored verbatim; never parsed by this package.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>UTC timestamp of the award.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public GamificationPlayer Player { get; set; } = null!;
}
