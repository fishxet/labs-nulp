package client;

import com.google.gson.annotations.Expose;

public class RetailCustomer extends Client {
    @Expose
    private double discount;

    public RetailCustomer(String id, String name, String email, double discount) {
        super(id, name, email);
        this.discount = discount;
    }

    public double getDiscount() { return discount; }
    public void setDiscount(double discount) { this.discount = discount; }

    @Override
    public double calculatePayment(double amount) {
        return amount * (1 - discount);
    }

    @Override
    public String toString() {
        return super.toString() + String.format(", Тип: Роздрібний, Знижка: %.0f%%", discount * 100);
    }
}