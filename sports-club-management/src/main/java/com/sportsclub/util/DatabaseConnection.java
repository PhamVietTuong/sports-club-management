package com.sportsclub.util;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;

/**
 * SINGLETON PATTERN — double-checked locking ensures a single DB connection.
 * SQL Server 2019+ with encrypted connection.
 */
public class DatabaseConnection {

    private static final String URL =
        "jdbc:sqlserver://localhost:1433;databaseName=SportsClubDB;" +
        "encrypt=true;trustServerCertificate=true";
    private static final String USER     = "sa";
    private static final String PASSWORD = "P@ssw0rd";

    // volatile guarantees visibility across threads (double-checked locking)
    private static volatile DatabaseConnection instance;
    private Connection connection;

    private DatabaseConnection() throws SQLException {
        try {
            Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
        } catch (ClassNotFoundException e) {
            throw new SQLException("SQL Server JDBC driver not found", e);
        }
        this.connection = DriverManager.getConnection(URL, USER, PASSWORD);
    }

    // SINGLETON — double-checked locking for thread-safe lazy initialization
    public static DatabaseConnection getInstance() throws SQLException {
        if (instance == null) {
            synchronized (DatabaseConnection.class) {
                if (instance == null) {
                    instance = new DatabaseConnection();
                }
            }
        }
        return instance;
    }

    public Connection getConnection() {
        try {
            if (connection == null || connection.isClosed()) {
                connection = DriverManager.getConnection(URL, USER, PASSWORD);
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return connection;
    }
}
