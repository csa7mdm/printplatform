using FluentAssertions;
using PrintPlatform.Gamification;

namespace PrintPlatform.Gamification.Tests;

public sealed class GamificationOptionsTests
{
    [Fact]
    public void Default_options_have_sensible_values()
    {
        var opts = new GamificationOptions();
        opts.PointsPerOrder.Should().BeGreaterThan(0);
        opts.BonusMultiplier.Should().BeGreaterThan(0);
    }
}
