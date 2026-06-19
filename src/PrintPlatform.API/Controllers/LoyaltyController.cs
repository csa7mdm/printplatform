using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PrintPlatform.Loyalty;
using PrintPlatform.Loyalty.Abstractions;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/loyalty")]
[Produces("application/json")]
public sealed class LoyaltyController : ApiControllerBase
{
    private readonly ILoyaltyService _loyalty;
    private readonly LoyaltyOptions _options;

    public LoyaltyController(
        ILoyaltyService loyalty,
        IOptions<LoyaltyOptions> options)
    {
        _loyalty = loyalty;
        _options = options.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(LoyaltyInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var externalUserId = CurrentUserId.ToString();

        var member = await _loyalty.GetMemberAsync(externalUserId, ct);
        var rewards = await _loyalty.GetRewardsAsync(ct);
        await _loyalty.GetLedgerAsync(externalUserId, ct: ct);

        var lifetimePoints = member?.LifetimePoints ?? 0;
        var response = new LoyaltyInfoResponse(
            member?.CurrentTier?.Name ?? ResolveTierName(lifetimePoints),
            member?.ActivePoints ?? 0,
            lifetimePoints,
            ResolveNextTierThreshold(lifetimePoints),
            rewards.Select(reward => new LoyaltyRewardResponse(
                reward.Id,
                reward.Name,
                reward.Description,
                reward.PointsCost,
                reward.Type,
                reward.Value)).ToArray());

        return Ok(response);
    }

    private string ResolveTierName(int lifetimePoints)
    {
        if (lifetimePoints >= _options.PlatinumThreshold)
            return "Platinum";

        if (lifetimePoints >= _options.GoldThreshold)
            return "Gold";

        if (lifetimePoints >= _options.SilverThreshold)
            return "Silver";

        return "Bronze";
    }

    private int? ResolveNextTierThreshold(int lifetimePoints)
    {
        if (lifetimePoints < _options.SilverThreshold)
            return _options.SilverThreshold;

        if (lifetimePoints < _options.GoldThreshold)
            return _options.GoldThreshold;

        if (lifetimePoints < _options.PlatinumThreshold)
            return _options.PlatinumThreshold;

        return null;
    }
}

public sealed record LoyaltyInfoResponse(
    string CurrentTier,
    int ActivePoints,
    int LifetimePoints,
    int? NextTierThreshold,
    IReadOnlyList<LoyaltyRewardResponse> Rewards);

public sealed record LoyaltyRewardResponse(
    long Id,
    string Name,
    string Description,
    int PointsCost,
    RewardType Type,
    double Value);
