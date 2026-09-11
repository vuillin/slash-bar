namespace SlashBar.Modules.Calendar;

/// <summary>
/// Decides whether a stored event occurs on a given calendar day,
/// based on its start <see cref="CalendarEvent.Date"/> and <see cref="CalendarEvent.Repeat"/>.
/// </summary>
public static class CalendarRecurrence {

    public const string Never = "Never";
    public const string EveryDay = "Every Day";
    public const string EveryWeek = "Every Week";
    public const string EveryTwoWeeks = "Every 2 Weeks";
    public const string EveryMonth = "Every Month";
    public const string EveryYear = "Every Year";


    public static bool OccursOn(CalendarEvent entry, DateTime day) {
        var start = entry.Date.Date;
        day = day.Date;

        // Recurrence never starts before the event's first day
        if (day < start)
            return false;

        return entry.Repeat switch {
            EveryDay => true,
            EveryWeek => (day - start).Days % 7 == 0,
            EveryTwoWeeks => (day - start).Days % 14 == 0,
            EveryMonth => IsSameMonthDay(start, day),
            EveryYear => IsSameYearDay(start, day),
            _ => day == start // Never, or unknown → only the start date
        };
    }


    // Same day-of-month; if that day doesn't exist (e.g. Jan 31 → Feb), use the last day of the month.
    private static bool IsSameMonthDay(DateTime start, DateTime day) {
        var targetDay = Math.Min(start.Day, DateTime.DaysInMonth(day.Year, day.Month));
        return day.Day == targetDay;
    }


    // Same month + day each year; Feb 29 clamps to Feb 28 on non-leap years.
    private static bool IsSameYearDay(DateTime start, DateTime day) {
        if (day.Month != start.Month)
            return false;

        var targetDay = Math.Min(start.Day, DateTime.DaysInMonth(day.Year, day.Month));
        return day.Day == targetDay;
    }
}
