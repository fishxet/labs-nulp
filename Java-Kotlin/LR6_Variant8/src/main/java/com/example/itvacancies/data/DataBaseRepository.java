package com.example.itvacancies.data;

import com.example.itvacancies.model.Vacancy;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class DataBaseRepository implements Repository {
    private final DataBaseConnector connector;

    public DataBaseRepository(DataBaseConnector connector) {
        this.connector = connector;
        // Створення таблиці
        String ddl = "CREATE TABLE IF NOT EXISTS Vacancies(" +
                "id IDENTITY PRIMARY KEY, " +
                "company VARCHAR(100), " +
                "position VARCHAR(100), " +
                "technology VARCHAR(50), " +
                "requirements CLOB, " +
                "salary DOUBLE)";
        try (Connection conn = connector.getConnection()) {
            conn.createStatement().execute(ddl);
        } catch (SQLException e) {
            throw new RuntimeException(e);
        }
    }

    @Override
    public List<Vacancy> getAll() {
        List<Vacancy> list = new ArrayList<>();
        String sql = "SELECT * FROM Vacancies";
        try (Connection conn = connector.getConnection();
             Statement st = conn.createStatement();
             ResultSet rs = st.executeQuery(sql)) {
            while (rs.next()) {
                list.add(map(rs));
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return list;
    }

    @Override
    public List<Vacancy> getByTechnology(String tech) {
        List<Vacancy> list = new ArrayList<>();
        String sql = "SELECT * FROM Vacancies WHERE technology LIKE ?";
        try (Connection conn = connector.getConnection();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setString(1, "%" + tech + "%");
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(map(rs));
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return list;
    }

    @Override
    public List<Vacancy> getByCompany(String comp) {
        List<Vacancy> list = new ArrayList<>();
        String sql = "SELECT * FROM Vacancies WHERE company LIKE ?";
        try (Connection conn = connector.getConnection();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setString(1, "%" + comp + "%");
            try (ResultSet rs = ps.executeQuery()) {
                while (rs.next()) list.add(map(rs));
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return list;
    }

    @Override
    public Vacancy getById(int id) {
        String sql = "SELECT * FROM Vacancies WHERE id = ?";
        try (Connection conn = connector.getConnection();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, id);
            try (ResultSet rs = ps.executeQuery()) {
                if (rs.next()) return map(rs);
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return null;
    }

    @Override
    public boolean add(Vacancy v) {
        String sql = "INSERT INTO Vacancies(company,position,technology,requirements,salary) VALUES(?,?,?,?,?)";
        try (Connection conn = connector.getConnection();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setString(1, v.getCompany());
            ps.setString(2, v.getPosition());
            ps.setString(3, v.getTechnology());
            ps.setString(4, v.getRequirements());
            ps.setDouble(5, v.getSalary());
            return ps.executeUpdate() > 0;
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return false;
    }

    @Override
    public boolean update(int id, Vacancy v) {
        String sql = "UPDATE Vacancies SET company=?,position=?,technology=?,requirements=?,salary=? WHERE id=?";
        try (Connection conn = connector.getConnection();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setString(1, v.getCompany());
            ps.setString(2, v.getPosition());
            ps.setString(3, v.getTechnology());
            ps.setString(4, v.getRequirements());
            ps.setDouble(5, v.getSalary());
            ps.setInt(6, id);
            return ps.executeUpdate() > 0;
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return false;
    }

    @Override
    public boolean delete(int id) {
        String sql = "DELETE FROM Vacancies WHERE id=?";
        try (Connection conn = connector.getConnection();
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setInt(1, id);
            return ps.executeUpdate() > 0;
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return false;
    }

    private Vacancy map(ResultSet rs) throws SQLException {
        return new Vacancy(
                rs.getInt("id"),
                rs.getString("company"),
                rs.getString("position"),
                rs.getString("technology"),
                rs.getString("requirements"),
                rs.getDouble("salary")
        );
    }
}