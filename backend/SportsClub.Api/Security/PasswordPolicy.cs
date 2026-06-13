using System.Text.RegularExpressions;

namespace SportsClub.Api.Security;

/// <summary>
/// SECURITY — central password strength policy. Requires a minimum length plus
/// at least one letter and one digit, so trivially weak passwords (e.g.
/// "12345678" or "password") are rejected. Port of the Java <c>PasswordPolicy</c>.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    /// <returns>null if the password is acceptable, otherwise a localized
    /// message describing why it was rejected.</returns>
    public static string? Validate(string? password)
    {
        if (password is null || password.Length < MinLength)
            return $"Mật khẩu phải có ít nhất {MinLength} ký tự.";
        if (!Regex.IsMatch(password, "[A-Za-z]"))
            return "Mật khẩu phải chứa ít nhất một chữ cái.";
        if (!Regex.IsMatch(password, "\\d"))
            return "Mật khẩu phải chứa ít nhất một chữ số.";
        return null;
    }

    public static bool IsValid(string? password) => Validate(password) is null;
}
