using PCDynasty.Club.Facilities;
using Xunit;

namespace PCDynasty.Club.Tests;

public sealed class FacilityStateTests
{
    [Fact]
    public void MaintenanceRestoresQualityAndConditionWithoutInventingModernization()
    {
        var facility = new FacilityState(
            "facility:training",
            "club:7",
            asOfDay: 1_000,
            quality: 700,
            condition: 800,
            modernization: 350);
        var plan = new MaintenancePlan(
            annualConditionDecay: 100,
            qualityRestoration: 20,
            conditionRestoration: 90);

        FacilityState maintained = facility.ApplyMaintenance(targetDay: 1_365, plan);

        Assert.Equal(720, maintained.Quality);
        Assert.Equal(790, maintained.Condition);
        Assert.Equal(350, maintained.Modernization);
        Assert.Equal(1_365, maintained.AsOfDay);
    }

    [Fact]
    public void ModernizationIsAnExplicitSeparateChange()
    {
        var facility = new FacilityState(
            "facility:academy",
            "club:7",
            asOfDay: 2_000,
            quality: 600,
            condition: 700,
            modernization: 300);

        FacilityState modernized = facility.Modernize(
            targetDay: 2_001,
            modernizationGain: 150,
            qualityGain: 50);

        Assert.Equal(650, modernized.Quality);
        Assert.Equal(700, modernized.Condition);
        Assert.Equal(450, modernized.Modernization);
        Assert.Throws<ArgumentOutOfRangeException>(() => facility.ApplyMaintenance(
            targetDay: 1_999,
            new MaintenancePlan(100, 10, 10)));
    }
}
