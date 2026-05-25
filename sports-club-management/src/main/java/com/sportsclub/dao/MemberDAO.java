package com.sportsclub.dao;

import com.sportsclub.model.Member;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

// DAO PATTERN — all DB operations for members table
public class MemberDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    public List<Member> findAll() throws SQLException {
        List<Member> list = new ArrayList<>();
        String sql = "SELECT m.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM members m INNER JOIN users u ON m.user_id = u.id " +
                     "ORDER BY m.join_date DESC";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public List<Member> findByStatus(String status) throws SQLException {
        List<Member> list = new ArrayList<>();
        // SQL INJECTION PREVENTION — parameterised query
        String sql = "SELECT m.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM members m INNER JOIN users u ON m.user_id = u.id " +
                     "WHERE m.status = ? ORDER BY m.join_date DESC";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, status);
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(mapRow(rs));
            }
        }
        return list;
    }

    public Member findById(int id) throws SQLException {
        String sql = "SELECT m.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM members m INNER JOIN users u ON m.user_id = u.id WHERE m.id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public Member findByUserId(int userId) throws SQLException {
        String sql = "SELECT m.*, u.username, u.email, u.phone, u.password_hash, u.created_at " +
                     "FROM members m INNER JOIN users u ON m.user_id = u.id WHERE m.user_id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, userId);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public int insert(int userId, String fullName, String gender,
                      LocalDate dateOfBirth, String address, int packageId,
                      LocalDate expiryDate) throws SQLException {
        String sql = "INSERT INTO members (user_id, full_name, gender, date_of_birth, " +
                     "address, package_id, expiry_date) VALUES (?, ?, ?, ?, ?, ?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(
                sql, Statement.RETURN_GENERATED_KEYS)) {
            ps.setInt(1, userId);
            ps.setString(2, fullName);
            ps.setString(3, gender);
            ps.setDate(4, dateOfBirth != null ? Date.valueOf(dateOfBirth) : null);
            ps.setString(5, address);
            ps.setInt(6, packageId);
            ps.setDate(7, expiryDate != null ? Date.valueOf(expiryDate) : null);
            ps.executeUpdate();
            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next()) return keys.getInt(1);
            }
        }
        return -1;
    }

    public void update(Member m) throws SQLException {
        String sql = "UPDATE members SET full_name=?, gender=?, date_of_birth=?, " +
                     "address=?, package_id=?, expiry_date=?, status=? WHERE id=?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, m.getFullName());
            ps.setString(2, m.getGender());
            ps.setDate(3, m.getDateOfBirth() != null ? Date.valueOf(m.getDateOfBirth()) : null);
            ps.setString(4, m.getAddress());
            ps.setInt(5, m.getPackageId());
            ps.setDate(6, m.getExpiryDate() != null ? Date.valueOf(m.getExpiryDate()) : null);
            ps.setString(7, m.getStatus());
            ps.setInt(8, m.getId());
            ps.executeUpdate();
        }
    }

    public void updateStatus(int id, String status) throws SQLException {
        String sql = "UPDATE members SET status = ? WHERE id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, status);
            ps.setInt(2, id);
            ps.executeUpdate();
        }
    }

    public int countAll() throws SQLException {
        String sql = "SELECT COUNT(*) FROM members";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            if (rs.next()) return rs.getInt(1);
        }
        return 0;
    }

    private Member mapRow(ResultSet rs) throws SQLException {
        Member m = new Member();
        m.setId(rs.getInt("id"));
        m.setRole(com.sportsclub.model.User.Role.MEMBER);
        m.setUsername(rs.getString("username"));
        m.setPasswordHash(rs.getString("password_hash"));
        m.setEmail(rs.getString("email"));
        m.setPhone(rs.getString("phone"));
        m.setFullName(rs.getString("full_name"));
        m.setGender(rs.getString("gender"));
        m.setAddress(rs.getString("address"));
        m.setPackageId(rs.getInt("package_id"));
        m.setStatus(rs.getString("status"));
        Date dob = rs.getDate("date_of_birth");
        if (dob != null) m.setDateOfBirth(dob.toLocalDate());
        Date join = rs.getDate("join_date");
        if (join != null) m.setJoinDate(join.toLocalDate());
        Date exp = rs.getDate("expiry_date");
        if (exp != null) m.setExpiryDate(exp.toLocalDate());
        Timestamp ts = rs.getTimestamp("created_at");
        if (ts != null) m.setCreatedAt(ts.toLocalDateTime());
        return m;
    }
}
