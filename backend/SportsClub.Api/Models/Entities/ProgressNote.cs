namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A coach's progress note / evaluation for a member (maps to the
/// <c>progress_notes</c> table). Optionally tied to a class and carries an
/// optional 1–5 rating. The member can read their own notes.
/// </summary>
public class ProgressNote
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int CoachId { get; set; }
    public int? ClassId { get; set; }
    public string Note { get; set; } = string.Empty;

    /// <summary>Optional 1–5 progress rating.</summary>
    public int? Rating { get; set; }
    public DateTime RecordedAt { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
}
