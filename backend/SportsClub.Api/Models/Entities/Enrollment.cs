namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A member's enrollment in a class (maps to the <c>enrollments</c> table).
/// Unique on (member_id, class_id).
/// </summary>
public class Enrollment
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int ClassId { get; set; }
    public DateOnly EnrollDate { get; set; }
    public string Status { get; set; } = "ACTIVE";

    // Navigation
    public Member Member { get; set; } = null!;
    public TrainingClass Class { get; set; } = null!;
}
