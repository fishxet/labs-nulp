#include <iostream>
#include <thread>
#include <vector>
#include <string>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <csignal>
#include <map>

#define BUFFER_SIZE 1024
#define DISCOVERY_PORT 8888  // Змінено порт
#define DISCOVERY_TIMEOUT 5

volatile sig_atomic_t client_running = 1;
int server_socket;
std::string username;

void signal_handler(int signal) {
    client_running = 0;
}

void receive_messages() {
    char buffer[BUFFER_SIZE + 1];
    while (client_running) {
        int bytes = recv(server_socket, buffer, BUFFER_SIZE, 0);
        if (bytes <= 0) {
            std::cerr << "З'єднання втрачено!" << std::endl;
            exit(1);
        }
        buffer[bytes] = '\0';
        std::cout << "\n" << buffer << std::endl;
        std::cout << ">> ";
        fflush(stdout);
    }
}

struct ServerInfo {
    std::string name;
    std::string ip;
    uint16_t port;
};

std::vector<ServerInfo> discover_servers() {
    int sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (sock < 0) {
        std::cerr << "Помилка створення UDP сокету!" << std::endl;
        exit(1);
    }
    int opt = 1;
    setsockopt(sock, SOL_SOCKET, SO_BROADCAST, &opt, sizeof(opt));
    setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt)); // Додано

    sockaddr_in addr;
    addr.sin_family = AF_INET;
    addr.sin_port = htons(DISCOVERY_PORT);
    addr.sin_addr.s_addr = INADDR_ANY;

    if (bind(sock, (sockaddr*)&addr, sizeof(addr)) == -1) {
        std::cerr << "Помилка прив'язки бродкаст-сокету!" << std::endl;
        close(sock);
        exit(1);
    }

    std::vector<ServerInfo> servers;
    std::map<std::string, bool> seen;

    fd_set readfds;
    struct timeval timeout;
    timeout.tv_sec = DISCOVERY_TIMEOUT;
    timeout.tv_usec = 0;

    std::cout << "Пошук серверів..." << std::endl;

    while (client_running) {  // Додано перевірку на client_running
        FD_ZERO(&readfds);
        FD_SET(sock, &readfds);

        int activity = select(sock + 1, &readfds, NULL, NULL, &timeout);
        if (activity <= 0) {
            std::cout << "Пошук завершено." << std::endl;
            break;
        }

        char buffer[BUFFER_SIZE];
        sockaddr_in server_addr;
        socklen_t addr_len = sizeof(server_addr);

        int bytes = recvfrom(sock, buffer, BUFFER_SIZE, 0, (sockaddr*)&server_addr, &addr_len);
        if (bytes <= 0) continue;
        buffer[bytes] = '\0';
        std::string msg(buffer);

        size_t pos1 = msg.find(';');
        size_t pos2 = msg.find(';', pos1 + 1);
        if (pos1 == std::string::npos || pos2 == std::string::npos) continue;

        ServerInfo info;
        info.name = msg.substr(0, pos1);
        info.ip = msg.substr(pos1 + 1, pos2 - pos1 - 1);
        info.port = static_cast<uint16_t>(std::stoi(msg.substr(pos2 + 1)));

        std::string key = info.ip + ":" + std::to_string(info.port);
        if (!seen[key]) {
            servers.push_back(info);
            seen[key] = true;
        }
    }

    close(sock);
    return servers;
}

int main() {
    signal(SIGINT, signal_handler);

    std::cout << "Введіть ваше ім'я: ";
    std::getline(std::cin, username);
    if (username.empty()) username = "Гість";

    auto servers = discover_servers();
    if (servers.empty()) {
        std::cerr << "Сервери не знайдено!" << std::endl;
        return 1;
    }

    std::cout << "Доступні сервери:" << std::endl;
    for (size_t i = 0; i < servers.size(); ++i) {
        std::cout << i + 1 << ". " << servers[i].name << " ["
                  << servers[i].ip << ":" << servers[i].port << "]" << std::endl;
    }

    int choice;
    std::cout << "Оберіть сервер (номер): ";
    std::cin >> choice;
    std::cin.ignore();
    if (choice < 1 || static_cast<size_t>(choice) > servers.size()) {
        std::cerr << "Невірний вибір." << std::endl;
        return 1;
    }

    ServerInfo selected = servers[choice - 1];

    server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        std::cerr << "Помилка створення сокету: " << strerror(errno) << std::endl;
        return 1;
    }

    sockaddr_in server_addr;
    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(selected.port);
    inet_pton(AF_INET, selected.ip.c_str(), &server_addr.sin_addr);

    if (connect(server_socket, (sockaddr*)&server_addr, sizeof(server_addr)) == -1) {
        std::cerr << "Помилка підключення до сервера: " << strerror(errno) << std::endl;
        return 1;
    }

    send(server_socket, username.c_str(), username.size(), 0);
    std::thread(receive_messages).detach();
    std::cout << "Підключено до сервера!" << std::endl;

    while (client_running) {
        std::string message;
        std::cout << ">> ";
        std::getline(std::cin, message);
        if (!message.empty()) {
            send(server_socket, message.c_str(), message.size(), 0);
        }
    }

    close(server_socket);
    std::cout << "Клієнт завершив роботу." << std::endl;
    return 0;
}