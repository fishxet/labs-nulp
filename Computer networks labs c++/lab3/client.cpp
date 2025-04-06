#include <iostream>
#include <thread>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <algorithm>
#include <csignal>

#define BUFFER_SIZE 1024          // msg

// Глобальні змінні
volatile sig_atomic_t client_running = 1; // client on/off ctrl+C
int server_socket;                      
std::string username;                  

// (Ctrl+C)
void signal_handler(int signal) {
    client_running = 0;  // client off
}

// Функція для отримання повідомлень від сервера
void receive_messages() {
    char buffer[BUFFER_SIZE + 1];  // Буфер (+1 для нуль-термінатора)
    while (client_running) {
        // Отримання даних від сервера
        int bytes = recv(server_socket, buffer, BUFFER_SIZE, 0);
        if (bytes <= 0) {
            std::cerr << "З'єднання втрачено!" << std::endl;
            exit(1);
        }
        buffer[bytes] = '\0';  // Додаємо термінатор
        // [username]: msg
        std::cout << "\n" << buffer << std::endl;
        // Оновлення підказки для вводу
        std::cout << ">> ";
        fflush(stdout);  // Важливо для коректного виводу в терміналі
    }
}

// server search
std::string discover_server() {
    // socket create
    int sock = socket(AF_INET, SOCK_DGRAM, 0);
    int opt = 1;
    // Встановлення опції для бродкастингу
    setsockopt(sock, SOL_SOCKET, SO_BROADCAST, &opt, sizeof(opt));

    // Крок 2: Налаштування адреси для прослуховування
    sockaddr_in addr;
    addr.sin_family = AF_INET;         // IPv4
    addr.sin_port = htons(8889);       // Порт для пошуку сервера
    addr.sin_addr.s_addr = INADDR_ANY; // Слухаємо всі інтерфейси

    // socket to list addr
    if (bind(sock, (sockaddr*)&addr, sizeof(addr)) == -1) {
        std::cerr << "Помилка прив'язки бродкаст-сокету!" << std::endl;
        exit(1);
    }

    // getmsg from serv const char* msg = "CHAT_SERVER";
    char buffer[BUFFER_SIZE];
    sockaddr_in server_addr;
    socklen_t len = sizeof(server_addr); 
    std::cout << "Пошук сервера..." << std::endl;
    int bytes = recvfrom(sock, buffer, BUFFER_SIZE, 0, (sockaddr*)&server_addr, &len);
    close(sock);  // Закриття сокета

    if (bytes <= 0) {
        std::cerr << "Сервер не знайдено!" << std::endl;
        exit(1);
    }

    // Повертаємо IP-адресу сервера
    return inet_ntoa(server_addr.sin_addr);
}

int main() {
    // Реєстрація обробника для Ctrl+C
    signal(SIGINT, signal_handler);

    // Крок 1: Введення імені користувача
    std::cout << "Введіть ваше ім'я: ";
    std::getline(std::cin, username);
    if (username.empty()) {
        username = "Гість";  // Значення за замовчуванням
    }

    // udp-broadcast
    std::string server_ip = discover_server();
    std::cout << "Знайдено сервер: " << server_ip << std::endl;

    // server socket init
    server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        std::cerr << "Помилка створення сокету: " << strerror(errno) << std::endl;
        return -1;
    }

    // Налаштування адреси сервера
    sockaddr_in server_addr;
    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(8888);  // Порт сервера
    inet_pton(AF_INET, server_ip.c_str(), &server_addr.sin_addr);

    // Встановлення з'єднання
    if (connect(server_socket, (sockaddr*)&server_addr, sizeof(server_addr)) == -1) {
        std::cerr << "Помилка підключення: " << strerror(errno) << std::endl;
        return -1;
    }

    // username to server
    send(server_socket, username.c_str(), username.size(), 0);

    // msg get to us
    std::thread(receive_messages).detach();
    std::cout << "Підключено до сервера!" << std::endl;

    // msg to serv
    while (client_running) {
        std::string message;
        std::cout << ">> ";
        std::getline(std::cin, message);

        // msg != null
        if (!message.empty()) {
            send(server_socket, message.c_str(), message.size(), 0);
        }
    }

    // close
    close(server_socket);
    std::cout << "Клієнт завершив роботу." << std::endl;
    return 0;
}