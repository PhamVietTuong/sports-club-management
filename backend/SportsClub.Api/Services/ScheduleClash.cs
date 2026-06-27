using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Services;

/// <summary>
/// Detects timetable clashes so a coach is never assigned two classes whose
/// weekly slots overlap. Two slots clash when they fall on the same day of week
/// and their time ranges overlap (<c>start1 &lt; end2 &amp;&amp; start2 &lt; end1</c>).
/// </summary>
public static class ScheduleClash
{
    /// <summary>
    /// Returns the first clashing pair between the <paramref name="incoming"/>
    /// slots (of a class being claimed) and the coach's <paramref name="existing"/>
    /// slots, or null if there is no clash.
    /// </summary>
    public static (Schedule incoming, Schedule existing)? FindConflict(
        IEnumerable<Schedule> incoming, IEnumerable<Schedule> existing)
    {
        foreach (var i in incoming)
            foreach (var e in existing)
                if (i.DayOfWeek == e.DayOfWeek
                    && i.StartTime < e.EndTime
                    && e.StartTime < i.EndTime)
                    return (i, e);
        return null;
    }

    /// <summary>
    /// Returns the coach's weekly class slot that a one-off session on
    /// <paramref name="date"/> from <paramref name="start"/> to
    /// <paramref name="end"/> clashes with (same weekday + overlapping time), or
    /// null if there is no clash. Used to keep a PT booking from colliding with
    /// the coach's teaching timetable.
    /// </summary>
    public static Schedule? FindClassClash(
        DateOnly date, TimeOnly start, TimeOnly end, IEnumerable<Schedule> classSchedules)
    {
        var weekday = date.DayOfWeek.ToString().ToUpperInvariant();
        foreach (var s in classSchedules)
            if (s.DayOfWeek == weekday && s.StartTime < end && start < s.EndTime)
                return s;
        return null;
    }

    /// <summary>Do two time ranges on the same day overlap?</summary>
    public static bool TimesOverlap(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2) =>
        start1 < end2 && start2 < end1;

    /// <summary>
    /// Returns an existing slot in the same room that a new slot (day/start/end)
    /// would clash with — so two classes can't occupy one room at once. Skips the
    /// slot whose id is <paramref name="excludeId"/> (the row being edited).
    /// </summary>
    public static Schedule? FindRoomClash(
        string? room, string day, TimeOnly start, TimeOnly end,
        IEnumerable<Schedule> sameRoomSchedules, int excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(room)) return null; // no room booked → nothing to clash
        foreach (var s in sameRoomSchedules)
            if (s.Id != excludeId && s.DayOfWeek == day && s.StartTime < end && start < s.EndTime)
                return s;
        return null;
    }
}
