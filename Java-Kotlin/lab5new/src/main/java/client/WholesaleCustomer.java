package client;

import com.google.gson.annotations.Expose;

public class WholesaleCustomer extends Client {
    @Expose
    private double minOrderVolume;

    public WholesaleCustomer(String id, String name, String email, double minOrderVolume) {
        super(id, name, email);
        this.minOrderVolume = minOrderVolume;
    }

    @Override
    public double calculatePayment(double amount) {
        return (amount >= minOrderVolume) ? amount * 0.9 : amount;
    }
}