namespace SportsClub.Api.Security;

/// <summary>
/// SECURITY — BCrypt password hashing. Cost factor 12 makes each hash ~300ms,
/// making brute-force attacks expensive. Port of the Java <c>BCryptUtil</c>;
/// BCrypt.Net-Next is wire-compatible with the existing $2a$ hashes in the DB.
/// </summary>
public static class PasswordHasher
{
    private const int WorkFactor = 12;

    public static string Hash(string plainText) =>
        BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);

    public static bool Verify(string plainText, string hash)
    {
        // A malformed/garbage stored hash must fail closed (return false), never
        // throw — an unhandled exception here would turn a bad login into a 500
        // and could leak that the account row exists.
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
