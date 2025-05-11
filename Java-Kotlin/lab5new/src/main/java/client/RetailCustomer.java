package client;

import com.google.gson.annotations.Expose;

public class RetailCustomer extends Client {
    @Expose
    private double discount;

    public RetailCustomer(String id, String name, String email, double discount) {
        super(id, name, email);
        if (discount < 0.0 || discount > 1.0) {
            throw new IllegalArgumentException("Discount must be in the range 0.0 to 1.0");
        }
        this.discount = discount;
    }

    public double getDiscount() {
        return discount;
    }

    public void setDiscount(double discount) {
        if (discount < 0.0 || discount > 1.0) {
            throw new IllegalArgumentException("Discount must be in the range 0.0 to 1.0");
        }
        this.discount = discount;
    }

    @Override
    public double calculatePayment(double amount) {
        if (amount < 0) {
            throw new IllegalArgumentException("Amount cannot be negative");
        }
        return amount * (1 - discount);
    }
}