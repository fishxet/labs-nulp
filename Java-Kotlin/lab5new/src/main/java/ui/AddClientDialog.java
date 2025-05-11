package ui;

import client.*;
import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import java.text.DecimalFormat;

public class AddClientDialog extends JDialog {
    private JComboBox<String> typeComboBox;
    private JTextField idField, nameField, emailField, extraField;
    private JButton okButton, cancelButton;
    private Client client;
    private boolean confirmed = false;
    private JLabel extraLabel;
    public AddClientDialog() {
        setTitle("Додати клієнта");
        setSize(400, 300);
        setModal(true);
        setLayout(new GridLayout(6, 2));

        // Инициализация компонентов
        typeComboBox = new JComboBox<>(new String[]{"Роздрібний", "Оптовий", "Партнер"});
        idField = new JTextField();
        nameField = new JTextField();
        emailField = new JTextField();
        extraField = new JTextField();
        extraLabel = new JLabel("Додатково:");  // Инициализация метки

        okButton = new JButton("OK");
        cancelButton = new JButton("Скасувати");

        // Добавление компонентов
        add(new JLabel("Тип:"));
        add(typeComboBox);
        add(new JLabel("ID:"));
        add(idField);
        add(new JLabel("Ім'я:"));
        add(nameField);
        add(new JLabel("Email:"));
        add(emailField);
        add(extraLabel);  // Добавляем метку вместо жестко заданного текста
        add(extraField);
        add(okButton);
        add(cancelButton);

        // Обработчики событий
        typeComboBox.addActionListener(e -> updateExtraFieldLabel());
        updateExtraFieldLabel();

        okButton.addActionListener(e -> {
            if (validateInput()) {
                confirmed = true;
                dispose();
            }
        });
        cancelButton.addActionListener(e -> dispose());
    }

    public AddClientDialog(Client existingClient) {
        this();
        idField.setText(existingClient.getId());
        nameField.setText(existingClient.getName());
        emailField.setText(existingClient.getEmail());

        if (existingClient instanceof RetailCustomer) {
            typeComboBox.setSelectedItem("Роздрібний");
            extraField.setText(new DecimalFormat("#.##").format(((RetailCustomer) existingClient).getDiscount()));
        } else if (existingClient instanceof WholesaleCustomer) {
            typeComboBox.setSelectedItem("Оптовий");
            extraField.setText(new DecimalFormat("#.##").format(((WholesaleCustomer) existingClient).getMinOrderVolume()));
        } else if (existingClient instanceof PartnerCompany) {
            typeComboBox.setSelectedItem("Партнер");
            extraField.setText(((PartnerCompany) existingClient).getContractNumber());
        }
        typeComboBox.addActionListener(e -> updateExtraFieldLabel());
        updateExtraFieldLabel();
    }

    private boolean validateInput() {
        // Загальні перевірки для всіх клієнтів
        if (nameField.getText().trim().isEmpty()) {
            showError("Введіть ім'я клієнта");
            return false;
        }

        if (!emailField.getText().matches("^[A-Za-z0-9+_.-]+@(.+)$")) {
            showError("Невірний формат email");
            return false;
        }

        // Перевірки для конкретних типів
        String type = (String) typeComboBox.getSelectedItem();
        try {
            switch (type) {
                case "Роздрібний":
                    double discount = Double.parseDouble(extraField.getText());
                    if (discount < 0 || discount > 0.5) {
                        showError("Знижка повинна бути від 0 до 50%");
                        return false;
                    }
                    break;

                case "Оптовий":
                    double minOrder = Double.parseDouble(extraField.getText());
                    if (minOrder <= 0) {
                        showError("Мінімальне замовлення має бути більше 0");
                        return false;
                    }
                    break;

                case "Партнер":
                    if (extraField.getText().trim().isEmpty()) {
                        showError("Введіть номер договору");
                        return false;
                    }
                    break;
            }
        } catch (NumberFormatException e) {
            showError("Невірний числовий формат");
            return false;
        }

        return true;
    }

    private void showError(String message) {
        JOptionPane.showMessageDialog(this, message, "Помилка", JOptionPane.ERROR_MESSAGE);
    }

    public Client getClient() {
        String type = (String) typeComboBox.getSelectedItem();
        String id = idField.getText().trim();
        String name = nameField.getText().trim();
        String email = emailField.getText().trim();
        String extra = extraField.getText().trim();

        switch (type) {
            case "Роздрібний":
                return new RetailCustomer(
                        id.isEmpty() ? generateId() : id,
                        name,
                        email,
                        Double.parseDouble(extra)
                );
            case "Оптовий":
                return new WholesaleCustomer(
                        id.isEmpty() ? generateId() : id,
                        name,
                        email,
                        Double.parseDouble(extra)
                );
            case "Партнер":
                return new PartnerCompany(
                        id.isEmpty() ? generateId() : id,
                        name,
                        email,
                        extra
                );
            default:
                return null;
        }
    }

    private String generateId() {
        return String.valueOf(System.currentTimeMillis() % 100000);
    }
    private void updateExtraFieldLabel() {
        String type = (String) typeComboBox.getSelectedItem();
        if (type != null) {
            switch (type) {
                case "Роздрібний":
                    extraLabel.setText("Знижка (0-0.5):");
                    break;
                case "Оптовий":
                    extraLabel.setText("Мін. замовлення:");
                    break;
                case "Партнер":
                    extraLabel.setText("Номер договору:");
                    break;
                default:
                    extraLabel.setText("Додатково:");
            }
        }
    }
    public boolean isConfirmed() {
        return confirmed;
    }
}