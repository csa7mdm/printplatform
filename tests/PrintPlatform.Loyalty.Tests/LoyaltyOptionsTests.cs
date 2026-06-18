using FluentAssertions;
using PrintPlatform.Loyalty;

namespace PrintPlatform.Loyalty.Tests;

public sealed class LoyaltyOptionsTests
{
    [Fact]
    public void Default_tier_thresholds_are_ascending()
    {
        var opts = new LoyaltyOptions();
        opts.SilverThreshold.Should().BeLessThan(opts.GoldThreshold);
        opts.GoldThreshold.Should().BeLessThan(opts.PlatinumThreshold);
    }

    [Fact]
    public void Default_cashback_is_positive()
    {
        var opts = new LoyaltyOptions();
        opts.CashBackPercent.Should().BeGreaterThan(0);
    }
}
