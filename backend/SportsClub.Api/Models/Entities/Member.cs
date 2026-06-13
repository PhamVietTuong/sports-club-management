using SportsClub.Api.Patterns.Prototype;

namespace SportsClub.Api.Models.Entities;

/// <summary>
/// Member profile (maps to the <c>members</c> table). Linked 1:1 to a
/// <see cref="User"/> via <see cref="UserId"/>.
/// PROTOTYPE PATTERN — cloneable so an admin can duplicate a member template.
/// </summary>
public class Member : ISportClubPrototype<Member>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int PackageId { get; set; }
    public DateOnly JoinDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string Status { get; set; } = "ACTIVE";

    // Navigation
    public User User { get; set; } = null!;

    // PROTOTYPE PATTERN — shallow copy (clones the profile fields only)
    public Member Clone() => (Member)MemberwiseClone();
}
