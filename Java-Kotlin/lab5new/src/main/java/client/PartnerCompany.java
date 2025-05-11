package client;

import com.google.gson.annotations.Expose;

public class PartnerCompany extends Client {
    @Expose
    private String contractNumber;

    public PartnerCompany(String id, String name, String email, String contractNumber) {
        super(id, name, email);
        this.contractNumber = contractNumber;
    }

    public String getContractNumber() { return contractNumber; }
    public void setContractNumber(String contractNumber) { this.contractNumber = contractNumber; }

    @Override
    public double calculatePayment(double amount) {
        return amount * 0.85;
    }

    @Override
    public String toString() {
        return super.toString() + String.format(", Тип: Партнер, Договір: %s", contractNumber);
    }
}