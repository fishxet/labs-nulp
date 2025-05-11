package client;

import com.google.gson.annotations.Expose;

public class WholesaleCustomer extends Client {
    @Expose
    private double minOrderVolume;

    public WholesaleCustomer(String id, String name, String email, double minOrderVolume) {
        super(id, name, email);
        this.minOrderVolume = minOrderVolume;
    }

    public double getMinOrderVolume() { return minOrderVolume; }
    public void setMinOrderVolume(double minOrderVolume) { this.minOrderVolume = minOrderVolume; }

    @Override
    public double calculatePayment(double amount) {
        return (amount >= minOrderVolume) ? amount * 0.9 : amount;
    }

    @Override
    public String toString() {
        return super.toString() + String.format(", Тип: Оптовий, Мін. замовлення: $%.2f", minOrderVolume);
    }
}