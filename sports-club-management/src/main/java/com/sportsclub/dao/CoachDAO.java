package com.sportsclub.dao;

import com.sportsclub.model.Coach;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

// DAO PATTERN — all DB operations for coaches table
public class CoachDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    public List<Coach> findAll() throws SQLException {
        List<Coach> list = new ArrayList<>();
        String sql = "SELECT c.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM coaches c INNER JOIN users u ON c.user_id = u.id";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public Coach findById(int id) throws SQLException {
        String sql = "SELECT c.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM coaches c INNER JOIN users u ON c.user_id = u.id WHERE c.id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public Coach findByUserId(int userId) throws SQLException {
        String sql = "SELECT c.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM coaches c INNER JOIN users u ON c.user_id = u.id WHERE c.user_id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, userId);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public int insert(int userId, String fullName, String specialization,
                      String bio, int experience, double salary) throws SQLException {
        String sql = "INSERT INTO coaches (user_id, full_name, specialization, bio, experience, salary) " +
                     "VALUES (?, ?, ?, ?, ?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(
                sql, Statement.RETURN_GENERATED_KEYS)) {
            ps.setInt(1, userId);
            ps.setString(2, fullName);
            ps.setString(3, specialization);
            ps.setString(4, bio);
            ps.setInt(5, experience);
            ps.setDouble(6, salary);
            ps.executeUpdate();
            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next()) return keys.getInt(1);
            }
        }
        return -1;
    }

    public void update(Coach c) throws SQLException {
        String sql = "UPDATE coaches SET full_name=?, specialization=?, bio=?, experience=?, salary=? WHERE id=?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, c.getFullName());
            ps.setString(2, c.getSpecialization());
            ps.setString(3, c.getBio());
            ps.setInt(4, c.getExperience());
            ps.setDouble(5, c.getSalary());
            ps.setInt(6, c.getId());
            ps.executeUpdate();
        }
    }

    public int countAll() throws SQLException {
        String sql = "SELECT COUNT(*) FROM coaches";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            if (rs.next()) return rs.getInt(1);
        }
        return 0;
    }

    private Coach mapRow(ResultSet rs) throws SQLException {
        Coach c = new Coach();
        c.setId(rs.getInt("id"));
        c.setRole(com.sportsclub.model.User.Role.COACH);
        c.setUsername(rs.getString("username"));
        c.setPasswordHash(rs.getString("password_hash"));
        c.setEmail(rs.getString("email"));
        c.setPhone(rs.getString("phone"));
        c.setFullName(rs.getString("full_name"));
        c.setSpecialization(rs.getString("specialization"));
        c.setBio(rs.getString("bio"));
        c.setExperience(rs.getInt("experience"));
        c.setSalary(rs.getDouble("salary"));
        Timestamp ts = rs.getTimestamp("created_at");
        if (ts != null) c.setCreatedAt(ts.toLocalDateTime());
        return c;
    }
}
