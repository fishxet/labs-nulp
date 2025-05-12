package com.example.itvacancies.data;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;

public class DataBaseConnector {
    private final String url;
    private final String user;
    private final String pass;

    public DataBaseConnector(String dbName) {
        this.url = "jdbc:h2:file:./" + dbName;
        this.user = "sa";
        this.pass = "";
    }

    public Connection getConnection() throws SQLException {
        return DriverManager.getConnection(url, user, pass);
    }
}