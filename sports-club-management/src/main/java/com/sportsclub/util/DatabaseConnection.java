package com.sportsclub.util;

import com.zaxxer.hikari.HikariConfig;
import com.zaxxer.hikari.HikariDataSource;

import java.sql.Connection;
import java.sql.SQLException;

/**
 * SINGLETON PATTERN — owns a single connection POOL (HikariCP) for the app.
 *
 * SECURITY / RELIABILITY:
 *  - Each caller gets its OWN pooled Connection (thread-safe) and MUST close it
 *    (try-with-resources). The old design shared one Connection across every
 *    request thread, which is not thread-safe and is a single point of failure.
 *  - Credentials are read from environment variables, never hardcoded secrets.
 *    Local-dev fallbacks are provided so the app still runs out of the box.
 *  - Set DB_TRUST_CERT=false in production so the SQL Server TLS certificate
 *    is actually validated (prevents man-in-the-middle).
 *
 * Environment variables: DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD,
 *                        DB_TRUST_CERT.
 */
public class DatabaseConnection {

    // volatile guarantees visibility across threads (double-checked locking)
    private static volatile DatabaseConnection instance;

    private final HikariDataSource dataSource;

    private DatabaseConnection() {
        String host      = env("DB_HOST", "localhost");
        String port      = env("DB_PORT", "1433");
        String dbName    = env("DB_NAME", "SportsClubDB");
        String user      = env("DB_USER", "sa");
        String password  = env("DB_PASSWORD", "P@ssw0rd");
        boolean trustCert = Boolean.parseBoolean(env("DB_TRUST_CERT", "true"));

        String url = String.format(
            "jdbc:sqlserver://%s:%s;databaseName=%s;encrypt=true;trustServerCertificate=%s",
            host, port, dbName, trustCert);

        HikariConfig config = new HikariConfig();
        config.setDriverClassName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
        config.setJdbcUrl(url);
        config.setUsername(user);
        config.setPassword(password);
        config.setMaximumPoolSize(10);
        config.setMinimumIdle(2);
        config.setConnectionTimeout(30_000);
        config.setPoolName("SportsClubPool");

        this.dataSource = new HikariDataSource(config);
    }

    // Read an environment variable, falling back to a local-dev default
    private static String env(String key, String defaultValue) {
        String value = System.getenv(key);
        return (value != null && !value.trim().isEmpty()) ? value : defaultValue;
    }

    // SINGLETON — double-checked locking for thread-safe lazy initialization
    public static DatabaseConnection getInstance() {
        if (instance == null) {
            synchronized (DatabaseConnection.class) {
                if (instance == null) {
                    instance = new DatabaseConnection();
                }
            }
        }
        return instance;
    }

    /**
     * Borrow a connection from the pool. The caller MUST close it
     * (use try-with-resources) so it is returned to the pool.
     */
    public Connection getConnection() throws SQLException {
        return dataSource.getConnection();
    }
}
