package ui;
import client.Client;
import client.RetailCustomer; // Добавлено
import client.WholesaleCustomer; // Добавлено
import client.PartnerCompany; // Добавлено
import data.ClientDataManager;
import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import java.io.IOException;
import java.util.Collections;
import java.util.List;
import java.io.*;
import java.lang.reflect.Type;
import java.util.*;

public class MainWindow extends JFrame {
    private DefaultListModel<Client> clientListModel = new DefaultListModel<>();
    private JList<Client> clientList = new JList<>(clientListModel);

    public MainWindow() {
        super("Керування клієнтами");
        setSize(800, 500);
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        setLocationRelativeTo(null);

        // Завантаження даних
        try {
            List<Client> clients = ClientDataManager.loadClients("clients.json");
            // Заміна addAll на цикл для Java 8
            for (Client client : clients) {
                clientListModel.addElement(client);
            }
        } catch (IOException e) {
            JOptionPane.showMessageDialog(this, "Файл не знайдено. Створено новий список.");
        }

        // Кнопки
        JButton addButton = new JButton("Додати");
        JButton editButton = new JButton("Редагувати");
        JButton deleteButton = new JButton("Видалити");
        JButton saveButton = new JButton("Зберегти");

        // Стилізація списку
        clientList.setCellRenderer(new DefaultListCellRenderer() {
            @Override
            public Component getListCellRendererComponent(JList<?> list, Object value, int index, boolean isSelected, boolean cellHasFocus) {
                Component c = super.getListCellRendererComponent(list, value, index, isSelected, cellHasFocus);
                if (value instanceof RetailCustomer) {
                    c.setBackground(new Color(220, 240, 255));
                } else if (value instanceof WholesaleCustomer) {
                    c.setBackground(new Color(255, 240, 220));
                } else if (value instanceof PartnerCompany) {
                    c.setBackground(new Color(220, 255, 220));
                }
                return c;
            }
        });

        // Розміщення компонентів
        JPanel buttonPanel = new JPanel(new FlowLayout());
        buttonPanel.add(addButton);
        buttonPanel.add(editButton);
        buttonPanel.add(deleteButton);
        buttonPanel.add(saveButton);

        add(new JScrollPane(clientList), BorderLayout.CENTER);
        add(buttonPanel, BorderLayout.SOUTH);

        // Обробники подій
        addButton.addActionListener(e -> openAddDialog());
        editButton.addActionListener(e -> openEditDialog());
        deleteButton.addActionListener(e -> deleteClient());
        saveButton.addActionListener(e -> saveData());
    }

    private void openAddDialog() {
        AddClientDialog dialog = new AddClientDialog();
        dialog.setVisible(true);
        if (dialog.isConfirmed()) {
            clientListModel.addElement(dialog.getClient());
        }
    }

    private void openEditDialog() {
        int selectedIndex = clientList.getSelectedIndex();
        if (selectedIndex == -1) return;

        Client selectedClient = clientListModel.getElementAt(selectedIndex);
        AddClientDialog dialog = new AddClientDialog(selectedClient);
        dialog.setVisible(true);
        if (dialog.isConfirmed()) {
            clientListModel.set(selectedIndex, dialog.getClient());
        }
    }

    private void deleteClient() {
        int selectedIndex = clientList.getSelectedIndex();
        if (selectedIndex != -1) {
            clientListModel.remove(selectedIndex);
        }
    }

    private void saveData() {
        try {
            List<Client> clients = Collections.list(clientListModel.elements());
            ClientDataManager.saveClients(clients, "clients.json");
            JOptionPane.showMessageDialog(this, "Дані збережено!");
        } catch (IOException ex) {
            JOptionPane.showMessageDialog(this, "Помилка: " + ex.getMessage(), "Помилка", JOptionPane.ERROR_MESSAGE);
        }
    }

    public static void main(String[] args) {
        try {
            String dataFile = "clients.json";
            if (new File(dataFile).exists()) {
                List<Client> clients = ClientDataManager.loadClients(dataFile);
                ClientDataManager.saveClients(clients, dataFile);
            }
        } catch (IOException e) {
            System.err.println("Ошибка конвертации данных: " + e.getMessage());
        }

        SwingUtilities.invokeLater(() -> {
            try {
                new MainWindow().setVisible(true);
            } catch (Exception e) {
                JOptionPane.showMessageDialog(null,
                        "Ошибка запуска приложения: " + e.getMessage(),
                        "Ошибка", JOptionPane.ERROR_MESSAGE);
            }
        });
        SwingUtilities.invokeLater(() -> new MainWindow().setVisible(true));
    }
}