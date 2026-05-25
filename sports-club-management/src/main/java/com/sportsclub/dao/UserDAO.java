package com.sportsclub.dao;

import com.sportsclub.model.User;
import com.sportsclub.model.Member;
import com.sportsclub.model.Coach;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.time.LocalDateTime;

/**
 * DAO PATTERN — handles all DB operations for the users table.
 * SECURITY: all queries use PreparedStatement to prevent SQL injection.
 */
public class UserDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    // SQL INJECTION PREVENTION — parameterised query with ?
    public User findByUsername(String username) throws SQLException {
        String sql = "SELECT * FROM users WHERE username = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, username);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public User findByEmail(String email) throws SQLException {
        String sql = "SELECT * FROM users WHERE email = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, email);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public User findById(int id) throws SQLException {
        String sql = "SELECT * FROM users WHERE id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    // Returns the generated PK
    public int insert(String username, String passwordHash,
                      String email, String phone, String role) throws SQLException {
        String sql = "INSERT INTO users (username, password_hash, email, phone, role) " +
                     "VALUES (?, ?, ?, ?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(
                sql, Statement.RETURN_GENERATED_KEYS)) {
            ps.setString(1, username);
            ps.setString(2, passwordHash);
            ps.setString(3, email);
            ps.setString(4, phone);
            ps.setString(5, role);
            ps.executeUpdate();
            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next()) return keys.getInt(1);
            }
        }
        return -1;
    }

    public void updatePassword(int userId, String newHash) throws SQLException {
        String sql = "UPDATE users SET password_hash = ? WHERE id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, newHash);
            ps.setInt(2, userId);
            ps.executeUpdate();
        }
    }

    // BRUTE-FORCE PROTECTION — log every login attempt
    public void logLoginAttempt(String username, String ip, boolean success) throws SQLException {
        String sql = "INSERT INTO login_attempts (username, ip_address, is_success) VALUES (?, ?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, username);
            ps.setString(2, ip);
            ps.setBoolean(3, success);
            ps.executeUpdate();
        }
    }

    // Count failed attempts in the last 15 minutes for brute-force lockout
    public int countRecentFailedAttempts(String username) throws SQLException {
        String sql = "SELECT COUNT(*) FROM login_attempts " +
                     "WHERE username = ? AND is_success = 0 " +
                     "AND attempt_time > DATEADD(MINUTE, -15, GETDATE())";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, username);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return rs.getInt(1);
            }
        }
        return 0;
    }

    private User mapRow(ResultSet rs) throws SQLException {
        String roleStr = rs.getString("role");
        // Return a lightweight User — full objects (Member/Coach) are fetched via their own DAOs
        Member u = new Member();
        u.setId(rs.getInt("id"));
        u.setUsername(rs.getString("username"));
        u.setPasswordHash(rs.getString("password_hash"));
        u.setEmail(rs.getString("email"));
        u.setPhone(rs.getString("phone"));
        u.setRole(User.Role.valueOf(roleStr));
        Timestamp ts = rs.getTimestamp("created_at");
        if (ts != null) u.setCreatedAt(ts.toLocalDateTime());
        return u;
    }
}
