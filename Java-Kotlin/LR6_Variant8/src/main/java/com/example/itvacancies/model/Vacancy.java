package com.example.itvacancies.model;

public class Vacancy {
    private int id;
    private String company;
    private String position;
    private String technology;
    private String requirements;
    private double salary;

    public Vacancy() {}

    public Vacancy(int id, String company, String position, String technology, String requirements, double salary) {
        this.id = id;
        this.company = company;
        this.position = position;
        this.technology = technology;
        this.requirements = requirements;
        this.salary = salary;
    }

    public Vacancy(String company, String position, String technology, String requirements, double salary) {
        this(0, company, position, technology, requirements, salary);
    }

    // Геттери та сеттери
    public int getId() { return id; }
    public void setId(int id) { this.id = id; }
    public String getCompany() { return company; }
    public void setCompany(String company) { this.company = company; }
    public String getPosition() { return position; }
    public void setPosition(String position) { this.position = position; }
    public String getTechnology() { return technology; }
    public void setTechnology(String technology) { this.technology = technology; }
    public String getRequirements() { return requirements; }
    public void setRequirements(String requirements) { this.requirements = requirements; }
    public double getSalary() { return salary; }
    public void setSalary(double salary) { this.salary = salary; }

    @Override
    public String toString() {
        return company + " - " + position + " [" + technology + "] ($" + salary + ")";
    }
}