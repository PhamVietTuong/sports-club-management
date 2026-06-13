namespace SportsClub.Api.Patterns.Singleton;

/// <summary>
/// SINGLETON PATTERN — owns the single source of truth for the database
/// connection string, a direct port of the Java <c>DatabaseConnection</c>
/// singleton (double-checked locking).
///
/// SECURITY:
///  - Credentials are read from environment variables, never hardcoded.
///    Local-dev fallbacks let the app run out of the box.
///  - Set DB_TRUST_CERT=false in production so the SQL Server TLS certificate
///    is validated (prevents man-in-the-middle).
///
/// Environment variables: DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD,
///                        DB_TRUST_CERT.
/// EF Core itself manages the connection pool; this type only assembles the
/// connection string once and shares it across the application.
/// </summary>
public sealed class DatabaseConfig
{
    // volatile guarantees visibility across threads (double-checked locking)
    private static volatile DatabaseConfig? _instance;
    private static readonly object Lock = new();

    public string ConnectionString { get; }

    private DatabaseConfig()
    {
        string host = Env("DB_HOST", "localhost");
        string port = Env("DB_PORT", "1433");
        string dbName = Env("DB_NAME", "SportsClubDB");
        string user = Env("DB_USER", "sa");
        string password = Env("DB_PASSWORD", "P@ssw0rd");
        bool trustCert = bool.Parse(Env("DB_TRUST_CERT", "true"));

        ConnectionString =
            $"Server={host},{port};Database={dbName};User Id={user};Password={password};" +
            $"Encrypt=True;TrustServerCertificate={trustCert};MultipleActiveResultSets=True";
    }

    // SINGLETON — double-checked locking for thread-safe lazy initialization
    public static DatabaseConfig Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (Lock)
                {
                    _instance ??= new DatabaseConfig();
                }
            }
            return _instance;
        }
    }

    private static string Env(string key, string defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
