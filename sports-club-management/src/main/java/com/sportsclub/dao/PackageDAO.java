package com.sportsclub.dao;

import com.sportsclub.model.TrainingPackage;
import com.sportsclub.util.DatabaseConnection;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

// DAO PATTERN — all DB operations for training_packages table
public class PackageDAO {

    private Connection getConn() throws SQLException {
        return DatabaseConnection.getInstance().getConnection();
    }

    public List<TrainingPackage> findAll() throws SQLException {
        List<TrainingPackage> list = new ArrayList<>();
        String sql = "SELECT * FROM training_packages ORDER BY id";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public List<TrainingPackage> findActive() throws SQLException {
        List<TrainingPackage> list = new ArrayList<>();
        String sql = "SELECT * FROM training_packages WHERE is_active = 1 ORDER BY price";
        try (PreparedStatement ps = getConn().prepareStatement(sql);
             ResultSet rs = ps.executeQuery()) {
            while (rs.next()) list.add(mapRow(rs));
        }
        return list;
    }

    public TrainingPackage findById(int id) throws SQLException {
        String sql = "SELECT * FROM training_packages WHERE id = ?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return mapRow(rs);
            }
        }
        return null;
    }

    public int save(TrainingPackage pkg) throws SQLException {
        String sql = "INSERT INTO training_packages (name, duration_months, price, max_classes, description, is_active) " +
                     "VALUES (?, ?, ?, ?, ?, ?)";
        try (PreparedStatement ps = getConn().prepareStatement(
                sql, Statement.RETURN_GENERATED_KEYS)) {
            ps.setString(1, pkg.getName());
            ps.setInt(2, pkg.getDurationMonths());
            ps.setDouble(3, pkg.getPrice());
            ps.setInt(4, pkg.getMaxClasses());
            ps.setString(5, pkg.getDescription());
            ps.setBoolean(6, pkg.isActive());
            ps.executeUpdate();
            try (ResultSet keys = ps.getGeneratedKeys()) {
                if (keys.next()) return keys.getInt(1);
            }
        }
        return -1;
    }

    public void update(TrainingPackage pkg) throws SQLException {
        String sql = "UPDATE training_packages SET name=?, duration_months=?, price=?, " +
                     "max_classes=?, description=?, is_active=? WHERE id=?";
        try (PreparedStatement ps = getConn().prepareStatement(sql)) {
            ps.setString(1, pkg.getName());
            ps.setInt(2, pkg.getDurationMonths());
            ps.setDouble(3, pkg.getPrice());
            ps.setInt(4, pkg.getMaxClasses());
            ps.setString(5, pkg.getDescription());
            ps.setBoolean(6, pkg.isActive());
            ps.setInt(7, pkg.getId());
            ps.executeUpdate();
        }
    }

    private TrainingPackage mapRow(ResultSet rs) throws SQLException {
        TrainingPackage p = new TrainingPackage();
        p.setId(rs.getInt("id"));
        p.setName(rs.getString("name"));
        p.setDurationMonths(rs.getInt("duration_months"));
        p.setPrice(rs.getDouble("price"));
        p.setMaxClasses(rs.getInt("max_classes"));
        p.setDescription(rs.getString("description"));
        p.setActive(rs.getBoolean("is_active"));
        return p;
    }
}
