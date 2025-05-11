package client;

import com.google.gson.annotations.Expose;

public abstract class Client {
    @Expose
    private String id;
    @Expose
    private String name;
    @Expose
    private String email;

    public Client(String id, String name, String email) {
        this.id = id;
        this.name = name;
        this.email = email;
    }

    // Геттери
    public String getId() { return id; }
    public String getName() { return name; }
    public String getEmail() { return email; }

    // Сеттер для email
    public void setEmail(String email) { this.email = email; }

    // Абстрактний метод
    public abstract double calculatePayment(double amount);

    @Override
    public String toString() {
        return String.format("%s (ID: %s, Email: %s)", name, id, email);
    }
}