namespace SportsClub.Api.Models.Entities;

/// <summary>
/// SECURITY — one row per login attempt (maps to <c>login_attempts</c>).
/// Used for brute-force lockout: 5 failures in 15 minutes locks the account.
/// </summary>
public class LoginAttempt
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime AttemptTime { get; set; }
    public bool IsSuccess { get; set; }
}
