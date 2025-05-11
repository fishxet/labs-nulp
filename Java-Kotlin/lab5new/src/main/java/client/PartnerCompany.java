package client;

import com.google.gson.annotations.Expose;

public class PartnerCompany extends Client {
    @Expose
    private String contractNumber;

    public PartnerCompany(String id, String name, String email, String contractNumber) {
        super(id, name, email);
        this.contractNumber = contractNumber;
    }

    @Override
    public double calculatePayment(double amount) {
        return amount * 0.85; // Фіксована знижка для партнерів
    }
}