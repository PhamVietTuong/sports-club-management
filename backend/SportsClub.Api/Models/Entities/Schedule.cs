using SportsClub.Api.Patterns.Prototype;

namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A weekly class schedule slot (maps to the <c>schedules</c> table).
/// PROTOTYPE PATTERN — clone this week's schedule into the next week.
/// </summary>
public class Schedule : ISportClubPrototype<Schedule>
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Room { get; set; }
    public bool RepeatWeekly { get; set; } = true;

    // Navigation
    public TrainingClass Class { get; set; } = null!;

    public Schedule Clone() => (Schedule)MemberwiseClone();
}
