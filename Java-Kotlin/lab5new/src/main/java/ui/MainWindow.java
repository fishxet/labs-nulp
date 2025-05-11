package ui;

import client.Client;
import data.ClientDataManager;
import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import java.util.Collections;
import java.util.List;

public class MainWindow extends JFrame {
    private DefaultListModel<Client> clientListModel;
    private JList<Client> clientList;
    private JButton addButton, editButton, deleteButton, saveButton;

    public MainWindow() {
        super("Керування клієнтами");
        setSize(600, 400);
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        setLocationRelativeTo(null);

        // Ініціалізація компонентів
        clientListModel = new DefaultListModel<>();
        clientList = new JList<>(clientListModel);
        addButton = new JButton("Додати");
        editButton = new JButton("Редагувати");
        deleteButton = new JButton("Видалити");
        saveButton = new JButton("Зберегти");

        // Завантаження даних
        try {
            List<Client> clients = ClientDataManager.loadClients("clients.json");
            clientListModel.addAll(clients);
        } catch (IOException e) {
            JOptionPane.showMessageDialog(this, "Файл не знайдено. Створено новий список.");
        }

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
        saveButton.addActionListener(e -> saveData());
        deleteButton.addActionListener(e -> deleteClient());
    }

    private void openAddDialog() {
        AddClientDialog dialog = new AddClientDialog();
        dialog.setVisible(true);
        if (dialog.isConfirmed()) {
            clientListModel.addElement(dialog.getClient());
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

    private void deleteClient() {
        int selectedIndex = clientList.getSelectedIndex();
        if (selectedIndex != -1) {
            clientListModel.remove(selectedIndex);
        }
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> new MainWindow().setVisible(true));
    }
}