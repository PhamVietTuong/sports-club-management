using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Security;

/// <summary>
/// SECURITY — issues signed JWT bearer tokens on login. Replaces the Java
/// server-side session: the SPA stores the token and sends it in the
/// Authorization header. The token carries the user id, username, role, and
/// (for members/coaches) their profile id so controllers can authorize and
/// resolve the caller without a DB round-trip.
///
/// Because auth is stateless and travels in the Authorization header (not a
/// cookie), classic CSRF does not apply — the browser never auto-attaches the
/// token to forged cross-site requests.
/// </summary>
public class JwtTokenService
{
    public const string ProfileIdClaim = "profileId";

    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings) => _settings = settings.Value;

    public (string token, DateTime expiresAt) CreateToken(User user, int? profileId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (profileId is not null)
            claims.Add(new Claim(ProfileIdClaim, profileId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
