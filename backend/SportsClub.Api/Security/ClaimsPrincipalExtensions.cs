using System.Security.Claims;

namespace SportsClub.Api.Security;

/// <summary>Convenience accessors for the claims baked into the JWT at login.</summary>
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Missing user id claim."));

    /// <summary>The member/coach profile id, or null for an admin.</summary>
    public static int? GetProfileId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(JwtTokenService.ProfileIdClaim);
        return int.TryParse(raw, out var id) ? id : null;
    }

    public static string GetUsername(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name) ?? "";
}
