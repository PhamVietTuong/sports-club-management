namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A member's rating of a coach (maps to the <c>coach_ratings</c> table).
/// One row per (member, coach) — re-rating updates the existing row.
/// </summary>
public class CoachRating
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int CoachId { get; set; }

    /// <summary>1–5 stars.</summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
}
