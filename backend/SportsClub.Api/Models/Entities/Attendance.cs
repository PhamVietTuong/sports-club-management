namespace SportsClub.Api.Models.Entities;

/// <summary>
/// One member's attendance for a class on a given day (maps to the
/// <c>attendance</c> table). Unique on (class_id, member_id, session_date).
/// A member can self check-in; a coach can mark/override the status.
/// </summary>
public class Attendance
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public int MemberId { get; set; }
    public DateOnly SessionDate { get; set; }

    /// <summary>PRESENT / ABSENT / LATE.</summary>
    public string Status { get; set; } = "PRESENT";

    /// <summary>Set when the member self check-in; null when the coach marked it manually.</summary>
    public DateTime? CheckedInAt { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
    public TrainingClass Class { get; set; } = null!;
}
