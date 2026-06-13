using SportsClub.Api.Patterns.Prototype;

namespace SportsClub.Api.Models.Entities;

/// <summary>
/// Shared authentication record for every role (maps to the <c>users</c> table).
/// PROTOTYPE PATTERN — implements Clone() via MemberwiseClone, mirroring the
/// Java <c>User</c> base which was the prototype root.
/// </summary>
public class User : ISportClubPrototype<User>
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = UserRole.Member;
    public DateTime CreatedAt { get; set; }

    // PROTOTYPE PATTERN — shallow copy
    public User Clone() => (User)MemberwiseClone();
}
