namespace SportsClub.Api.Security;

/// <summary>Strongly-typed JWT configuration (bound from the "Jwt" section).</summary>
public class JwtSettings
{
    public string Issuer { get; set; } = "SportsClubApi";
    public string Audience { get; set; } = "SportsClubClient";
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 30;
}
