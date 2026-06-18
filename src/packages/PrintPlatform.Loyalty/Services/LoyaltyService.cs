using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PrintPlatform.Loyalty.Abstractions;
using PrintPlatform.Loyalty.Data;
using PrintPlatform.Loyalty.Events;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Services;

public sealed class LoyaltyService : ILoyaltyService
{
    private readonly LoyaltyDbContext _db;
    private readonly IMediator _mediator;
    private readonly LoyaltyOptions _options;
    private readonly ITierStrategy _tierStrategy;

    public LoyaltyService(
        LoyaltyDbContext db,
        IMediator mediator,
        IOptions<LoyaltyOptions> options,
        ITierStrategy tierStrategy)
    {
        _db = db;
        _mediator = mediator;
        _options = options.Value;
        _tierStrategy = tierStrategy;
    }

    public async Task<LoyaltyMember> EnsureMemberAsync(
        string externalUserId,
        DateOnly? dateOfBirth = null,
        CancellationToken ct = default)
    {
        var member = await _db.Members
            .Include(m => m.CurrentTier)
            .FirstOrDefaultAsync(m => m.ExternalUserId == externalUserId, ct);

        if (member is null)
        {
            var tiers = await _db.Tiers.ToListAsync(ct);
            var initialTier = _tierStrategy.Evaluate(0, tiers);

            member = new LoyaltyMember
            {
                ExternalUserId = externalUserId,
                DateOfBirth = dateOfBirth,
                CurrentTierId = initialTier.Id,
                CurrentTier = initialTier,
                ReferralCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.Members.Add(member);
            await _db.SaveChangesAsync(ct);
        }

        return member;
    }

    public async Task<EarnResult> EarnAsync(
        string externalUserId,
        decimal orderAmountEgp,
        string? orderReference = null,
        bool hasDesignService = false,
        int? bonusPoints = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var member = await _db.Members
            .Include(m => m.CurrentTier)
            .FirstOrDefaultAsync(m => m.ExternalUserId == externalUserId, ct);

        if (member is null)
            member = await EnsureMemberAsync(externalUserId, ct: ct);

        var tiers = await _db.Tiers.ToListAsync(ct);
        
        // Calculate points
        double multiplier = member.CurrentTier?.Multiplier ?? 1.0;
        if (hasDesignService) multiplier *= _options.DesignServiceMultiplier;
        
        // Check birthday month
        if (member.DateOfBirth.HasValue && member.DateOfBirth.Value.Month == DateTime.Today.Month)
            multiplier *= _options.BirthdayMultiplier;

        int basePoints = (int)Math.Floor((double)orderAmountEgp * _options.PointsPerEgp * multiplier);
        int totalAwarded = basePoints + (bonusPoints ?? 0);

        if (member.CompletedOrderCount == 0 && _options.FirstOrderBonusPoints > 0)
        {
            totalAwarded += _options.FirstOrderBonusPoints;
        }

        // Update member
        member.ActivePoints += totalAwarded;
        member.LifetimePoints += totalAwarded;
        if (orderAmountEgp > 0) member.CompletedOrderCount++;
        member.UpdatedAt = DateTimeOffset.UtcNow;

        // Ledger entry
        var entry = new PointsLedger
        {
            MemberId = member.Id,
            Delta = totalAwarded,
            Type = (bonusPoints > 0 || orderAmountEgp == 0) ? LedgerEntryType.Bonus : LedgerEntryType.Earned,
            OrderReference = orderReference,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.LedgerEntries.Add(entry);

        // Tier evaluation
        var newTier = _tierStrategy.Evaluate(member.LifetimePoints, tiers);
        string? newTierName = null;

        if (newTier.Id != member.CurrentTierId)
        {
            var oldTierName = member.CurrentTier?.Name ?? "None";
            bool upgraded = newTier.SortOrder > (member.CurrentTier?.SortOrder ?? -1);
            member.CurrentTierId = newTier.Id;
            member.CurrentTier = newTier;
            newTierName = newTier.Name;

            if (upgraded)
                await _mediator.Publish(new TierUpgradedEvent(externalUserId, oldTierName, newTier.Name, member.LifetimePoints, DateTimeOffset.UtcNow), ct);
            else
                await _mediator.Publish(new TierDowngradedEvent(externalUserId, oldTierName, newTier.Name, member.LifetimePoints, DateTimeOffset.UtcNow), ct);
        }

        await _db.SaveChangesAsync(ct);
        await _mediator.Publish(new PointsEarnedEvent(externalUserId, totalAwarded, member.ActivePoints, member.LifetimePoints, reason, DateTimeOffset.UtcNow), ct);

        return new EarnResult(
            externalUserId,
            basePoints,
            totalAwarded - basePoints,
            totalAwarded,
            member.ActivePoints,
            member.LifetimePoints,
            newTierName);
    }

    public async Task<RedemptionResult> RedeemAsync(
        string externalUserId,
        long rewardId,
        string? orderReference = null,
        CancellationToken ct = default)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.ExternalUserId == externalUserId, ct)
            ?? throw new InvalidOperationException("Member not found.");

        var reward = await _db.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId && r.IsActive, ct)
            ?? throw new InvalidOperationException("Active reward not found.");

        if (member.ActivePoints < reward.PointsCost)
            throw new InsufficientPointsException(reward.PointsCost, member.ActivePoints);

        member.ActivePoints -= reward.PointsCost;
        member.UpdatedAt = DateTimeOffset.UtcNow;

        var redemption = new Redemption
        {
            MemberId = member.Id,
            RewardId = reward.Id,
            PointsUsed = reward.PointsCost,
            OrderReference = orderReference,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Redemptions.Add(redemption);

        var entry = new PointsLedger
        {
            MemberId = member.Id,
            Delta = -reward.PointsCost,
            Type = LedgerEntryType.Redeemed,
            OrderReference = orderReference,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.LedgerEntries.Add(entry);

        await _db.SaveChangesAsync(ct);
        await _mediator.Publish(new RewardRedeemedEvent(externalUserId, redemption.Id, reward.Id, reward.Name, reward.Type, reward.Value, reward.PointsCost, orderReference, DateTimeOffset.UtcNow), ct);

        return new RedemptionResult(
            redemption.Id,
            reward.Name,
            reward.Type,
            reward.Value,
            reward.PointsCost,
            member.ActivePoints);
    }

    public async Task<LoyaltyMember?> GetMemberAsync(string externalUserId, CancellationToken ct = default)
        => await _db.Members.Include(m => m.CurrentTier).FirstOrDefaultAsync(m => m.ExternalUserId == externalUserId, ct);

    public async Task<IReadOnlyList<PointsLedger>> GetLedgerAsync(string externalUserId, int pageSize = 50, int page = 0, CancellationToken ct = default)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.ExternalUserId == externalUserId, ct);
        if (member is null) return Array.Empty<PointsLedger>();

        return await _db.LedgerEntries
            .Where(l => l.MemberId == member.Id)
            .OrderByDescending(l => l.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reward>> GetRewardsAsync(CancellationToken ct = default)
        => await _db.Rewards.Where(r => r.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Redemption>> GetRedemptionsAsync(string externalUserId, CancellationToken ct = default)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.ExternalUserId == externalUserId, ct);
        if (member is null) return Array.Empty<Redemption>();

        return await _db.Redemptions
            .Include(r => r.Reward)
            .Where(r => r.MemberId == member.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }
}
