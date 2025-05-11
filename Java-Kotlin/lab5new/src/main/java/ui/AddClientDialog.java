package ui;

import client.*;
import javax.swing.*;
import java.awt.*;
import java.awt.event.*;

public class AddClientDialog extends JDialog {
    private JComboBox<String> typeComboBox;
    private JTextField idField, nameField, emailField, extraField;
    private JButton okButton, cancelButton;
    private Client client;
    private boolean confirmed = false;

    public AddClientDialog() {
        setTitle("Додати клієнта");
        setSize(400, 300);
        setModal(true);
        setLayout(new GridLayout(6, 2));

        // Поля для введення
        typeComboBox = new JComboBox<>(new String[]{"Роздрібний", "Оптовий", "Партнер"});
        idField = new JTextField();
        nameField = new JTextField();
        emailField = new JTextField();
        extraField = new JTextField();

        // Кнопки
        okButton = new JButton("OK");
        cancelButton = new JButton("Скасувати");

        // Додавання компонентів
        add(new JLabel("Тип:"));
        add(typeComboBox);
        add(new JLabel("ID:"));
        add(idField);
        add(new JLabel("Ім'я:"));
        add(nameField);
        add(new JLabel("Email:"));
        add(emailField);
        add(new JLabel("Додатково:"));
        add(extraField);
        add(okButton);
        add(cancelButton);

        // Обробники
        okButton.addActionListener(e -> {
            if (validateInput()) {
                confirmed = true;
                dispose();
            }
        });
        cancelButton.addActionListener(e -> dispose());
    }

    private boolean validateInput() {
        String email = emailField.getText();
        if (!email.contains("@")) {
            JOptionPane.showMessageDialog(this, "Невірний email!", "Помилка", JOptionPane.ERROR_MESSAGE);
            return false;
        }
        return true;
    }

    public Client getClient() {
        String type = (String) typeComboBox.getSelectedItem();
        String id = idField.getText();
        String name = nameField.getText();
        String email = emailField.getText();
        String extra = extraField.getText();

        switch (type) {
            case "Роздрібний":
                return new RetailCustomer(id, name, email, Double.parseDouble(extra));
            case "Оптовий":
                return new WholesaleCustomer(id, name, email, Double.parseDouble(extra));
            case "Партнер":
                return new PartnerCompany(id, name, email, extra);
            default:
                return null;
        }
    }

    public boolean isConfirmed() {
        return confirmed;
    }
}