#include <iostream>
#include <vector>
#include <thread>
#include <mutex>
#include <unordered_map> 
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <algorithm>
#include <csignal>

#define PORT 8888           
#define MAX_CLIENTS 10       
#define BUFFER_SIZE 1024     // Розмір буфера для обміну даними

struct ClientInfo {
    int socket;             // Файловий дескриптор сокета клієнта
    std::string username;  
};

// Глобальні змінні
std::vector<ClientInfo> clients;    
std::mutex clients_mutex;           // М'ютекс для синхронізації доступу до списку
volatile sig_atomic_t server_running = 1; // Server off/on

// Прототипи функцій
void handle_client(int client_socket);
void broadcast_server();
void signal_handler(int signal);

// (Ctrl+C)
void signal_handler(int signal) {
    server_running = 0;  // Server off
}

// msg
void handle_client(int client_socket) {
    char buffer[BUFFER_SIZE];          
    std::string username;              

    // client username get
    int bytes_received = recv(client_socket, buffer, BUFFER_SIZE, 0);
    if (bytes_received <= 0) {
        close(client_socket);  // error close
        return;
    }
    buffer[bytes_received] = '\0';
    username = buffer;

    // clients list + 1
    {
        std::lock_guard<std::mutex> lock(clients_mutex); // Блокування м'ютекса
        clients.push_back({client_socket, username});    // client add
    }

    std::cout << "Клієнт " << username << " підключився." << std::endl;

    // msg main loop
    while (server_running) {
        bytes_received = recv(client_socket, buffer, BUFFER_SIZE, 0);
        if (bytes_received <= 0) {
            break;  // exit if connection lost
        }
        buffer[bytes_received] = '\0';
        std::string message = "[" + username + "]: " + buffer; // Форматування

        // all clients gets msg
        std::lock_guard<std::mutex> lock(clients_mutex);
        for (const auto& client : clients) {
            if (client.socket != client_socket) { // sender
                send(client.socket, message.c_str(), message.size(), 0);
            }
        }
    }

    // clients--
    {
        std::lock_guard<std::mutex> lock(clients_mutex);
        // Видаляємо клієнта за допомогою лямбда-функції
        clients.erase(std::remove_if(clients.begin(), clients.end(),
            [client_socket](const ClientInfo& ci) { 
                return ci.socket == client_socket; 
            }), clients.end());
    }
    close(client_socket);  // Закриття сокета
    std::cout << "Клієнт " << username << " відключився." << std::endl;
}

// UDP server
void broadcast_server() {
    // POSIX: socket(AF_INET, SOCK_STREAM, 0) Windows: WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, NULL, 0, 0)
    int sock = socket(AF_INET, SOCK_DGRAM, 0);
    int broadcast_enable = 1;
    // Встановлюємо опцію  широкомовлення для сокета
    // для Windows потрібно явно встановити опцію SO_BROADCAST через setsockopt.
    // SO_BROADCAST - premit sending msg via broadcast
    // SOL_SOCKET - socket lvl
    
    setsockopt(sock, SOL_SOCKET, SO_BROADCAST, &broadcast_enable, sizeof(broadcast_enable));
    
    // Address for broadcast
    sockaddr_in addr;
    memset(&addr, 0, sizeof(addr));
    addr.sin_family = AF_INET;           // IPv4
    addr.sin_port = htons(8889);         // Порт для бродкастингу
    addr.sin_addr.s_addr = inet_addr("255.255.255.255"); // Широкомовна адреса
    
    const char* msg = "CHAT_SERVER"; // for us connection

    // Цикл відправки повідомлень
    while (server_running) {
        sendto(sock, msg, strlen(msg), 0, (sockaddr*)&addr, sizeof(addr));
        sleep(2);  // Пауза між відправками (2 секунди)
    }
    close(sock);  // Закриття сокета
}

int main() {
    // (Ctrl+C)
    signal(SIGINT, signal_handler);

    // TCP-сокет
    int server_fd = socket(AF_INET, SOCK_STREAM, 0);
    if (server_fd == -1) {
        std::cerr << "Помилка створення сокету: " << strerror(errno) << std::endl;
        return -1;
    }

    // re-use адреси
    int opt = 1;
    setsockopt(server_fd, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    // Налаштування адреси сервера
    sockaddr_in server_addr;
    server_addr.sin_family = AF_INET;           // IPv4
    server_addr.sin_port = htons(PORT);         // Вказаний порт
    server_addr.sin_addr.s_addr = INADDR_ANY;   // Всі доступні інтерфейси

    // link сокет до адреси
    if (bind(server_fd, (sockaddr*)&server_addr, sizeof(server_addr)) == -1) {
        std::cerr << "Помилка прив'язки: " << strerror(errno) << std::endl;
        return -1;
    }

    // connected users
    if (listen(server_fd, MAX_CLIENTS) == -1) {
        std::cerr << "Помилка listen: " << strerror(errno) << std::endl;
        return -1;
    }

    std::cout << "Сервер запущено на порті " << PORT << std::endl;

    // broadcast run
    std::thread(&broadcast_server).detach();

    // conecting
    while (server_running) {
        sockaddr_in client_addr;           // Адреса клієнта
        socklen_t client_len = sizeof(client_addr);
        // Прийняття нового підключення
        int client_fd = accept(server_fd, (sockaddr*)&client_addr, &client_len);

        if (client_fd == -1) {
            std::cerr << "Помилка accept: " << strerror(errno) << std::endl;
            continue;
        }

        // Запуск обробки клієнта в окремому потоці
        std::thread(handle_client, client_fd).detach();
    }

    close(server_fd);  // Закриття серверного сокета
    std::cout << "Сервер зупинено." << std::endl;
    return 0;
}