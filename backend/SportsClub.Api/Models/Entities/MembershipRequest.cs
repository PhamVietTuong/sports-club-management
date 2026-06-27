namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A member's request to register/purchase a training package (maps to the
/// <c>membership_requests</c> table). This is the lifecycle source of truth:
///
///   PENDING  → member submitted; awaiting admin decision
///   APPROVED → admin approved; member may still cancel/change for a grace
///              window (24h) while the membership is not yet active
///   ACTIVE   → membership activated (member activated it, or the first class
///              was registered/checked-in) — locked, no more cancel/change
///   REJECTED → admin rejected (terminal)
///   CANCELLED→ member cancelled within the grace window, or replaced by a
///              changed request (terminal)
/// </summary>
public class MembershipRequest
{
    /// <summary>Hours after approval during which the member may still cancel/change.</summary>
    public const int GraceWindowHours = 24;

    public int Id { get; set; }
    public int MemberId { get; set; }
    public int PackageId { get; set; }

    /// <summary>Price snapshot at request time (charged when the package is activated).</summary>
    public decimal Amount { get; set; }

    /// <summary>CASH / CARD / TRANSFER — chosen at request time.</summary>
    public string Method { get; set; } = "CASH";

    /// <summary>PENDING / APPROVED / ACTIVE / REJECTED / CANCELLED.</summary>
    public string Status { get; set; } = "PENDING";

    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>The day the membership started (set on activation).</summary>
    public DateOnly? StartDate { get; set; }
    public DateTime? ActivatedAt { get; set; }

    /// <summary>Optional admin note (e.g. rejection reason).</summary>
    public string? Note { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
    public TrainingPackage Package { get; set; } = null!;

    /// <summary>
    /// Whether the member may still cancel or change this request: always while
    /// PENDING, and while APPROVED only inside the 24h grace window. Once the
    /// membership is ACTIVE (first class used / explicitly activated) it is locked.
    /// </summary>
    public bool IsModifiable() =>
        Status == "PENDING"
        || (Status == "APPROVED" && ApprovedAt is { } a && DateTime.Now < a.AddHours(GraceWindowHours));
}
