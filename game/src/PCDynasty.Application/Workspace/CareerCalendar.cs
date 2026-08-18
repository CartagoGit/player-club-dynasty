namespace PCDynasty.Application.Workspace;

/// <summary>
/// The date the career is currently living in.
/// </summary>
/// <remarks>
/// Presentation used to derive ages from a literal year written in a UI
/// helper (audit a00001), which silently aged the whole world every real-world
/// January. The calendar is now injected, so there is exactly one source for
/// "when are we". It is still seeded from the process clock at career start;
/// proposal `f00033` replaces that source with the deterministic world
/// orchestrator, and nothing outside this type needs to change when it does.
/// </remarks>
public sealed record CareerCalendar
{
    public CareerCalendar(DateOnly currentDate)
    {
        CurrentDate = currentDate;
    }

    public DateOnly CurrentDate { get; }

    public static CareerCalendar StartingAt(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        return new CareerCalendar(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
    }

    /// <summary>Completed years between <paramref name="birthDate"/> and today.</summary>
    public int AgeOf(DateOnly birthDate)
    {
        int age = CurrentDate.Year - birthDate.Year;
        // The birthday has not arrived yet this year.
        if (birthDate.AddYears(age) > CurrentDate)
        {
            age--;
        }

        return Math.Max(0, age);
    }

    /// <summary>
    /// Age when the content only declares a birth year. The result is the age
    /// the person reaches during the current year, not a precise age.
    /// </summary>
    public int AgeOf(int birthYear) => Math.Max(0, CurrentDate.Year - birthYear);
}
