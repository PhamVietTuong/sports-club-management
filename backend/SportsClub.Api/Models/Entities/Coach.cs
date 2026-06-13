using SportsClub.Api.Patterns.Prototype;

namespace SportsClub.Api.Models.Entities;

/// <summary>
/// Coach profile (maps to the <c>coaches</c> table). Linked 1:1 to a
/// <see cref="User"/> via <see cref="UserId"/>.
/// PROTOTYPE PATTERN — cloneable domain object.
/// </summary>
public class Coach : ISportClubPrototype<Coach>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public int Experience { get; set; }
    public decimal Salary { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public Coach Clone() => (Coach)MemberwiseClone();
}
