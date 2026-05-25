package com.sportsclub.dao;

import com.sportsclub.model.TrainingClass;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

// DAO PATTERN — all DB operations for training_classes table
public class ClassDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    public List<TrainingClass> findAll() throws SQLException {
        List<TrainingClass> list = new ArrayList<>();
        String sql = "SELECT tc.*, c.full_name AS coach_name FROM training_classes tc " +
                     "LEFT JOIN coaches c ON tc.coach_id = c.id ORDER BY tc.id";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public List<TrainingClass> findActive() throws SQLException {
        List<TrainingClass> list = new ArrayList<>();
        String sql = "SELECT tc.*, c.full_name AS coach_name FROM training_classes tc " +
                     "LEFT JOIN coaches c ON tc.coach_id = c.id WHERE tc.is_active = 1";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public List<TrainingClass> findByCoachId(int coachId) throws SQLException {
        List<TrainingClass> list = new ArrayList<>();
        String sql = "SELECT tc.*, c.full_name AS coach_name FROM training_classes tc " +
                     "LEFT JOIN coaches c ON tc.coach_id = c.id WHERE tc.coach_id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, coachId);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public TrainingClass findById(int id) throws SQLException {
        String sql = "SELECT tc.*, c.full_name AS coach_name FROM training_classes tc " +
                     "LEFT JOIN coaches c ON tc.coach_id = c.id WHERE tc.id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public int insert(TrainingClass tc) throws SQLException {
        String sql = "INSERT INTO training_classes (name, coach_id, capacity, level, description, is_active) " +
                     "VALUES (?, ?, ?, ?, ?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(
                sql, Statement.RETURN_GENERATED_KEYS)) {
            ps.setString(1, tc.getName());
            ps.setInt(2, tc.getCoachId());
            ps.setInt(3, tc.getCapacity());
            ps.setString(4, tc.getLevel());
            ps.setString(5, tc.getDescription());
            ps.setBoolean(6, tc.isActive());
            ps.executeUpdate();
            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next()) return keys.getInt(1);
            }
        }
        return -1;
    }

    public void update(TrainingClass tc) throws SQLException {
        String sql = "UPDATE training_classes SET name=?, coach_id=?, capacity=?, " +
                     "level=?, description=?, is_active=? WHERE id=?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, tc.getName());
            ps.setInt(2, tc.getCoachId());
            ps.setInt(3, tc.getCapacity());
            ps.setString(4, tc.getLevel());
            ps.setString(5, tc.getDescription());
            ps.setBoolean(6, tc.isActive());
            ps.setInt(7, tc.getId());
            ps.executeUpdate();
        }
    }

    public void incrementEnrolled(int classId) throws SQLException {
        String sql = "UPDATE training_classes SET current_enrolled = current_enrolled + 1 WHERE id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, classId);
            ps.executeUpdate();
        }
    }

    public void decrementEnrolled(int classId) throws SQLException {
        String sql = "UPDATE training_classes SET current_enrolled = current_enrolled - 1 WHERE id = ? AND current_enrolled > 0";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, classId);
            ps.executeUpdate();
        }
    }

    public int countAll() throws SQLException {
        String sql = "SELECT COUNT(*) FROM training_classes WHERE is_active = 1";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            if (rs.next()) return rs.getInt(1);
        }
        return 0;
    }

    private TrainingClass mapRow(ResultSet rs) throws SQLException {
        TrainingClass tc = new TrainingClass();
        tc.setId(rs.getInt("id"));
        tc.setName(rs.getString("name"));
        tc.setCoachId(rs.getInt("coach_id"));
        tc.setCoachName(rs.getString("coach_name"));
        tc.setCapacity(rs.getInt("capacity"));
        tc.setCurrentEnrolled(rs.getInt("current_enrolled"));
        tc.setLevel(rs.getString("level"));
        tc.setDescription(rs.getString("description"));
        tc.setActive(rs.getBoolean("is_active"));
        return tc;
    }
}
