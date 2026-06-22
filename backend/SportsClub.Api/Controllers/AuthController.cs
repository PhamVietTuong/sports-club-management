using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers;

/// <summary>
/// SECURITY — authentication controller. Implements BCrypt verification,
/// brute-force lockout after 5 failures in 15 min, generic error messages
/// (no account enumeration), and JWT issuance. Port of LoginServlet,
/// RegisterServlet and LogoutServlet.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const int MaxAttempts = 5;

    // A BCrypt hash of a throwaway password, computed once at startup (cost 12,
    // matching PasswordHasher). When the username does not exist we still run a
    // verify against this so a failed login costs the same time regardless of
    // whether the username is real — closing the timing side-channel that would
    // otherwise let an attacker enumerate valid usernames.
    private static readonly string DummyHash = PasswordHasher.Hash("dummy-password-not-used");

    private readonly UserRepository _users;
    private readonly MemberRepository _members;
    private readonly CoachRepository _coaches;
    private readonly JwtTokenService _jwt;
    private readonly AccountService _account;

    public AuthController(UserRepository users, MemberRepository members,
        CoachRepository coaches, JwtTokenService jwt, AccountService account)
    {
        _users = users;
        _members = members;
        _coaches = coaches;
        _jwt = jwt;
        _account = account;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        // BRUTE-FORCE PROTECTION — reject if too many recent failures
        if (await _users.CountRecentFailedAttemptsAsync(req.Username) >= MaxAttempts)
            return Unauthorized(new MessageResponse(
                "Tài khoản tạm thời bị khóa. Vui lòng đợi 15 phút."));

        var user = await _users.FindByUsernameAsync(req.Username);

        // BCrypt verification — constant-time inside BCrypt. When the user does
        // not exist, verify against a dummy hash anyway so a failed login costs
        // the same regardless of whether the username is real (anti-enumeration
        // via timing). The result for a missing user is always a failure.
        bool passwordOk = PasswordHasher.Verify(req.Password, user?.PasswordHash ?? DummyHash);
        if (user is null || !passwordOk)
        {
            await _users.LogLoginAttemptAsync(req.Username, ip, false);
            return Unauthorized(new MessageResponse(
                "Tên đăng nhập hoặc mật khẩu không đúng."));
        }

        await _users.LogLoginAttemptAsync(req.Username, ip, true);

        // Resolve the role-specific profile id + display name for the token/response.
        int? profileId = null;
        string fullName = user.Username;
        if (user.Role == UserRole.Member)
        {
            var m = await _members.FindByUserIdAsync(user.Id);
            profileId = m?.Id;
            fullName = m?.FullName ?? user.Username;
        }
        else if (user.Role == UserRole.Coach)
        {
            var c = await _coaches.FindByUserIdAsync(user.Id);
            profileId = c?.Id;
            fullName = c?.FullName ?? user.Username;
        }

        var (token, expiresAt) = _jwt.CreateToken(user, profileId);
        return Ok(new AuthResponse(token, expiresAt, user.Id, user.Username, user.Role, fullName));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Email)
            || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new MessageResponse("Vui lòng điền đầy đủ các trường bắt buộc."));

        if (req.Password != req.ConfirmPassword)
            return BadRequest(new MessageResponse("Mật khẩu không khớp."));

        // Shared account creation (password policy + generic-message duplicate
        // check + BCrypt hash + insert) lives in AccountService.
        var result = await _account.CreateUserAsync(
            req.Username, req.Email, req.Password, req.Phone, UserRole.Member);
        if (result.Error is not null)
            return StatusCode(result.StatusCode, new MessageResponse(result.Error));

        await _members.InsertAsync(result.UserId!.Value, req.FullName.Trim(),
            req.Gender, null, null, 0, null);

        return Ok(new MessageResponse("Đăng ký thành công! Vui lòng đăng nhập."));
    }

    /// <summary>
    /// Stateless logout. With JWT the server holds no session, so the client
    /// simply discards the token; this endpoint exists for symmetry/auditing.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() => Ok(new MessageResponse("Đã đăng xuất."));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _users.FindByIdAsync(User.GetUserId());
        if (user is null) return Unauthorized();

        string fullName = user.Username;
        if (user.Role == UserRole.Member)
            fullName = (await _members.FindByUserIdAsync(user.Id))?.FullName ?? fullName;
        else if (user.Role == UserRole.Coach)
            fullName = (await _coaches.FindByUserIdAsync(user.Id))?.FullName ?? fullName;

        return Ok(new { user.Id, user.Username, user.Role, FullName = fullName });
    }
}
