package com.sportsclub.dao;

import com.sportsclub.model.Enrollment;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

// DAO PATTERN — all DB operations for enrollments table
public class EnrollmentDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    public List<Enrollment> findByMemberId(int memberId) throws SQLException {
        List<Enrollment> list = new ArrayList<>();
        String sql = "SELECT e.*, tc.name AS class_name, m.full_name AS member_name " +
                     "FROM enrollments e " +
                     "INNER JOIN training_classes tc ON e.class_id = tc.id " +
                     "INNER JOIN members m ON e.member_id = m.id " +
                     "WHERE e.member_id = ? ORDER BY e.enroll_date DESC";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, memberId);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public List<Enrollment> findByClassId(int classId) throws SQLException {
        List<Enrollment> list = new ArrayList<>();
        String sql = "SELECT e.*, tc.name AS class_name, m.full_name AS member_name " +
                     "FROM enrollments e " +
                     "INNER JOIN training_classes tc ON e.class_id = tc.id " +
                     "INNER JOIN members m ON e.member_id = m.id " +
                     "WHERE e.class_id = ? AND e.status = 'ACTIVE'";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, classId);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public boolean isEnrolled(int memberId, int classId) throws SQLException {
        String sql = "SELECT COUNT(*) FROM enrollments WHERE member_id=? AND class_id=? AND status='ACTIVE'";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, memberId);
            ps.setInt(2, classId);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return rs.getInt(1) > 0;
            }
        }
        return false;
    }

    public void insert(int memberId, int classId) throws SQLException {
        String sql = "INSERT INTO enrollments (member_id, class_id) VALUES (?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, memberId);
            ps.setInt(2, classId);
            ps.executeUpdate();
        }
    }

    public void cancel(int memberId, int classId) throws SQLException {
        String sql = "UPDATE enrollments SET status='CANCELLED' WHERE member_id=? AND class_id=?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, memberId);
            ps.setInt(2, classId);
            ps.executeUpdate();
        }
    }

    private Enrollment mapRow(ResultSet rs) throws SQLException {
        Enrollment e = new Enrollment();
        e.setId(rs.getInt("id"));
        e.setMemberId(rs.getInt("member_id"));
        e.setMemberName(rs.getString("member_name"));
        e.setClassId(rs.getInt("class_id"));
        e.setClassName(rs.getString("class_name"));
        Date d = rs.getDate("enroll_date");
        if (d != null) e.setEnrollDate(d.toLocalDate());
        e.setStatus(rs.getString("status"));
        return e;
    }
}
