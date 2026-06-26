namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A personal-training session a member books with a coach (maps to the
/// <c>pt_sessions</c> table). The coach confirms/cancels/completes it.
/// </summary>
public class PtSession
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int CoachId { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    /// <summary>PENDING / CONFIRMED / CANCELLED / COMPLETED.</summary>
    public string Status { get; set; } = "PENDING";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
    public Coach Coach { get; set; } = null!;
}
