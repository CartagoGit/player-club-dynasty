using PCDynasty.Application.Workspace;
using Xunit;

namespace PCDynasty.Application.Tests.Workspace;

/// <summary>
/// Ages used to come from a literal year written inside a UI helper, so the
/// whole world aged every real-world January (audit a00001). The calendar is
/// the single source, and the boundary cases are the point of the type.
/// </summary>
public sealed class CareerCalendarTests
{
    private static readonly CareerCalendar Calendar = new(new DateOnly(2026, 8, 18));

    [Fact]
    public void AgeIsNotYetIncrementedTheDayBeforeTheBirthday()
    {
        Assert.Equal(25, Calendar.AgeOf(new DateOnly(2000, 8, 19)));
    }

    [Fact]
    public void AgeIncrementsOnTheBirthdayItself()
    {
        Assert.Equal(26, Calendar.AgeOf(new DateOnly(2000, 8, 18)));
    }

    [Fact]
    public void AgeUsesTheYearWhenTheContentOnlyDeclaresOne()
    {
        Assert.Equal(26, Calendar.AgeOf(2000));
    }

    [Fact]
    public void AnUnbornDateNeverProducesANegativeAge()
    {
        Assert.Equal(0, Calendar.AgeOf(new DateOnly(2030, 1, 1)));
        Assert.Equal(0, Calendar.AgeOf(2030));
    }

    [Fact]
    public void TheCalendarIsSeededFromTheInjectedClock()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2031, 3, 4, 10, 0, 0, TimeSpan.Zero));

        CareerCalendar calendar = CareerCalendar.StartingAt(clock);

        Assert.Equal(new DateOnly(2031, 3, 4), calendar.CurrentDate);
    }

    [Fact]
    public void ALeapDayBirthdayAgesOnTheLastDayOfFebruaryInACommonYear()
    {
        // A 29 February birthday has no exact anniversary in a common year;
        // the calendar settles it on 28 February rather than 1 March.
        Assert.Equal(26, new CareerCalendar(new DateOnly(2027, 2, 27)).AgeOf(new DateOnly(2000, 2, 29)));
        Assert.Equal(27, new CareerCalendar(new DateOnly(2027, 2, 28)).AgeOf(new DateOnly(2000, 2, 29)));
        Assert.Equal(28, new CareerCalendar(new DateOnly(2028, 2, 29)).AgeOf(new DateOnly(2000, 2, 29)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
