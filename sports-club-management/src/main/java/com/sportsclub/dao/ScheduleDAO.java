package com.sportsclub.dao;

import com.sportsclub.model.Schedule;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

// DAO PATTERN — all DB operations for schedules table
public class ScheduleDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    public List<Schedule> findAll() throws SQLException {
        List<Schedule> list = new ArrayList<>();
        String sql = "SELECT s.*, tc.name AS class_name FROM schedules s " +
                     "INNER JOIN training_classes tc ON s.class_id = tc.id ORDER BY s.id";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public List<Schedule> findByClassId(int classId) throws SQLException {
        List<Schedule> list = new ArrayList<>();
        String sql = "SELECT s.*, tc.name AS class_name FROM schedules s " +
                     "INNER JOIN training_classes tc ON s.class_id = tc.id WHERE s.class_id = ?";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, classId);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public List<Schedule> findByCoachId(int coachId) throws SQLException {
        List<Schedule> list = new ArrayList<>();
        String sql = "SELECT s.*, tc.name AS class_name FROM schedules s " +
                     "INNER JOIN training_classes tc ON s.class_id = tc.id " +
                     "WHERE tc.coach_id = ?";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, coachId);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public List<Schedule> findByMemberId(int memberId) throws SQLException {
        List<Schedule> list = new ArrayList<>();
        String sql = "SELECT s.*, tc.name AS class_name FROM schedules s " +
                     "INNER JOIN training_classes tc ON s.class_id = tc.id " +
                     "INNER JOIN enrollments e ON e.class_id = tc.id " +
                     "WHERE e.member_id = ? AND e.status = 'ACTIVE'";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, memberId);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public Schedule findById(int id) throws SQLException {
        String sql = "SELECT s.*, tc.name AS class_name FROM schedules s " +
                     "INNER JOIN training_classes tc ON s.class_id = tc.id WHERE s.id = ?";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public int save(Schedule s) throws SQLException {
        String sql = "INSERT INTO schedules (class_id, day_of_week, start_time, end_time, room, repeat_weekly) " +
                     "VALUES (?, ?, ?, ?, ?, ?)";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(
                sql, Statement.RETURN_GENERATED_KEYS)) {
            ps.setInt(1, s.getClassId());
            ps.setString(2, s.getDayOfWeek());
            ps.setTime(3, Time.valueOf(s.getStartTime()));
            ps.setTime(4, Time.valueOf(s.getEndTime()));
            ps.setString(5, s.getRoom());
            ps.setBoolean(6, s.isRepeatWeekly());
            ps.executeUpdate();
            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next()) return keys.getInt(1);
            }
        }
        return -1;
    }

    public void update(Schedule s) throws SQLException {
        String sql = "UPDATE schedules SET class_id=?, day_of_week=?, start_time=?, end_time=?, room=?, repeat_weekly=? WHERE id=?";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, s.getClassId());
            ps.setString(2, s.getDayOfWeek());
            ps.setTime(3, Time.valueOf(s.getStartTime()));
            ps.setTime(4, Time.valueOf(s.getEndTime()));
            ps.setString(5, s.getRoom());
            ps.setBoolean(6, s.isRepeatWeekly());
            ps.setInt(7, s.getId());
            ps.executeUpdate();
        }
    }

    public void delete(int id) throws SQLException {
        String sql = "DELETE FROM schedules WHERE id = ?";
        try (Connection conn = getConn();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, id);
            ps.executeUpdate();
        }
    }

    private Schedule mapRow(ResultSet rs) throws SQLException {
        Schedule s = new Schedule();
        s.setId(rs.getInt("id"));
        s.setClassId(rs.getInt("class_id"));
        s.setClassName(rs.getString("class_name"));
        s.setDayOfWeek(rs.getString("day_of_week"));
        Time start = rs.getTime("start_time");
        if (start != null) s.setStartTime(start.toLocalTime());
        Time end = rs.getTime("end_time");
        if (end != null) s.setEndTime(end.toLocalTime());
        s.setRoom(rs.getString("room"));
        s.setRepeatWeekly(rs.getBoolean("repeat_weekly"));
        return s;
    }
}
