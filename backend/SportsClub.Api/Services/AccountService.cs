using SportsClub.Api.Repositories;
using SportsClub.Api.Security;

namespace SportsClub.Api.Services;

/// <summary>
/// Single owner of "create a login" logic, shared by registration and the admin
/// add-member / add-coach / clone-member flows. Centralising it keeps the
/// password policy, the duplicate check, and — importantly — the SINGLE generic
/// "already in use" message (account-enumeration prevention) consistent across
/// every entry point, instead of being copy-pasted and drifting.
/// </summary>
public class AccountService
{
    private readonly UserRepository _users;

    public AccountService(UserRepository users) => _users = users;

    /// <param name="UserId">The new user's id on success, otherwise null.</param>
    /// <param name="Error">A localized error message on failure, otherwise null.</param>
    /// <param name="StatusCode">HTTP status the caller should return on failure.</param>
    public record Result(int? UserId, string? Error, int StatusCode);

    /// <summary>
    /// Validates the password policy, checks username/email uniqueness with one
    /// generic message, hashes the password (BCrypt cost 12) and inserts the user.
    /// </summary>
    public async Task<Result> CreateUserAsync(
        string username, string email, string password, string? phone, string role)
    {
        var pwError = PasswordPolicy.Validate(password);
        if (pwError is not null) return new Result(null, pwError, StatusCodes.Status400BadRequest);

        username = username.Trim();
        email = email.Trim();

        // ACCOUNT ENUMERATION PREVENTION — one generic message for either field.
        if (await _users.FindByUsernameAsync(username) is not null
            || await _users.FindByEmailAsync(email) is not null)
            return new Result(null, "Tên đăng nhập hoặc email đã được sử dụng.",
                StatusCodes.Status409Conflict);

        var hash = PasswordHasher.Hash(password);
        var userId = await _users.InsertAsync(username, hash, email, phone, role);
        return new Result(userId, null, StatusCodes.Status200OK);
    }
}
