namespace SportsClub.Api.Models.Entities;

/// <summary>The three roles, matching the <c>users.role</c> CHECK constraint.</summary>
public static class UserRole
{
    public const string Admin = "ADMIN";
    public const string Coach = "COACH";
    public const string Member = "MEMBER";
}
