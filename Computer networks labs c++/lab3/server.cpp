#include <iostream>
#include <vector>
#include <thread>
#include <mutex>
#include <algorithm>
#include <csignal>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <random>
#include <netdb.h>

#define MAX_CLIENTS 10
#define BUFFER_SIZE 1024
#define BROADCAST_PORT 8888  

struct ClientInfo {
    int socket;
    std::string username;
};

std::vector<ClientInfo> clients;
std::mutex clients_mutex;
volatile sig_atomic_t server_running = 1;

std::string server_name = "ChatServer";
int tcp_port;

void signal_handler(int signal) {
    server_running = 0;
}

void handle_client(int client_socket) {
    char buffer[BUFFER_SIZE];
    std::string username;

    int bytes_received = recv(client_socket, buffer, BUFFER_SIZE, 0);
    if (bytes_received <= 0) {
        close(client_socket);
        return;
    }
    buffer[bytes_received] = '\0';
    username = buffer;

    {
        std::lock_guard<std::mutex> lock(clients_mutex);
        clients.push_back({client_socket, username});
    }

    std::cout << "Клієнт " << username << " підключився." << std::endl;

    while (server_running) {
        bytes_received = recv(client_socket, buffer, BUFFER_SIZE, 0);
        if (bytes_received <= 0) break;
        buffer[bytes_received] = '\0';
        std::string message = "[" + username + "]: " + buffer;

        std::lock_guard<std::mutex> lock(clients_mutex);
        for (const auto& client : clients) {
            if (client.socket != client_socket) {
                send(client.socket, message.c_str(), message.size(), 0);
            }
        }
    }

    {
        std::lock_guard<std::mutex> lock(clients_mutex);
        clients.erase(std::remove_if(clients.begin(), clients.end(),
            [client_socket](const ClientInfo& ci) { return ci.socket == client_socket; }),
            clients.end());
    }
    close(client_socket);
    std::cout << "Клієнт " << username << " відключився." << std::endl;
}

std::string get_local_ip() {
    char buffer[INET_ADDRSTRLEN];
    sockaddr_in temp;
    socklen_t len = sizeof(temp);

    int temp_sock = socket(AF_INET, SOCK_DGRAM, 0);
    temp.sin_family = AF_INET;
    temp.sin_port = htons(53);
    inet_pton(AF_INET, "8.8.8.8", &temp.sin_addr);
    connect(temp_sock, (sockaddr*)&temp, sizeof(temp));
    getsockname(temp_sock, (sockaddr*)&temp, &len);
    inet_ntop(AF_INET, &temp.sin_addr, buffer, sizeof(buffer));
    std::string ip(buffer);
    close(temp_sock);
    return ip;
}

int get_random_port() {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> dis(10000, 60000);
    return dis(gen);
}

void broadcast_server() {
    int sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (sock < 0) {
        std::cerr << "Помилка створення UDP сокету для бродкасту!" << std::endl;
        return;
    }
    int broadcast_enable = 1;
    setsockopt(sock, SOL_SOCKET, SO_BROADCAST, &broadcast_enable, sizeof(broadcast_enable));

    sockaddr_in addr;
    memset(&addr, 0, sizeof(addr));
    addr.sin_family = AF_INET;
    addr.sin_port = htons(BROADCAST_PORT);
    addr.sin_addr.s_addr = inet_addr("255.255.255.255");

    std::string ip = get_local_ip();
    std::string message = server_name + ";" + ip + ";" + std::to_string(tcp_port);

    while (server_running) {
        sendto(sock, message.c_str(), message.size(), 0, (sockaddr*)&addr, sizeof(addr));
        sleep(2);
    }
    close(sock);
}

int main(int argc, char* argv[]) {
    signal(SIGINT, signal_handler);

    tcp_port = (argc > 1) ? std::stoi(argv[1]) : get_random_port();
    server_name = (argc > 2) ? argv[2] : server_name;

    int server_fd = socket(AF_INET, SOCK_STREAM, 0);
    if (server_fd == -1) {
        std::cerr << "Помилка створення сокету: " << strerror(errno) << std::endl;
        return -1;
    }

    int opt = 1;
    setsockopt(server_fd, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    sockaddr_in server_addr;
    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(tcp_port);
    server_addr.sin_addr.s_addr = INADDR_ANY;

    if (bind(server_fd, (sockaddr*)&server_addr, sizeof(server_addr)) == -1) {
        std::cerr << "Помилка прив'язки: " << strerror(errno) << std::endl;
        return -1;
    }

    if (listen(server_fd, MAX_CLIENTS) == -1) {
        std::cerr << "Помилка listen: " << strerror(errno) << std::endl;
        return -1;
    }

    std::cout << "Сервер запущено на порті " << tcp_port << std::endl;
    std::thread(broadcast_server).detach();

    while (server_running) {
        sockaddr_in client_addr;
        socklen_t client_len = sizeof(client_addr);
        int client_fd = accept(server_fd, (sockaddr*)&client_addr, &client_len);
        if (client_fd == -1) {
            if (server_running) std::cerr << "Помилка accept: " << strerror(errno) << std::endl;
            continue;
        }
        std::thread(handle_client, client_fd).detach();
    }

    close(server_fd);
    std::cout << "Сервер зупинено." << std::endl;
    return 0;
}