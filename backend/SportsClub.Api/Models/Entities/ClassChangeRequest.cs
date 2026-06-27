namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A coach's request to accept (CLAIM) or give up (RELEASE) a class, pending
/// admin approval (maps to the <c>class_change_requests</c> table). The class
/// assignment only changes once an admin approves — coaches no longer self-serve
/// claim/release directly.
///
///   PENDING  → coach submitted; awaiting admin decision
///   APPROVED → admin approved; the claim/release was applied
///   REJECTED → admin rejected (terminal)
/// </summary>
public class ClassChangeRequest
{
    public int Id { get; set; }
    public int CoachId { get; set; }
    public int ClassId { get; set; }

    /// <summary>CLAIM (accept an unassigned class) / RELEASE (give up an owned class).</summary>
    public string Action { get; set; } = "CLAIM";

    /// <summary>PENDING / APPROVED / REJECTED.</summary>
    public string Status { get; set; } = "PENDING";

    public DateTime RequestedAt { get; set; }
    public DateTime? DecidedAt { get; set; }

    /// <summary>Optional admin note (e.g. rejection reason).</summary>
    public string? Note { get; set; }

    // Navigation
    public Coach Coach { get; set; } = null!;
    public TrainingClass Class { get; set; } = null!;
}
