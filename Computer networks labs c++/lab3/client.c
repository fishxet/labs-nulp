#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>

#define PORT 8889
#define BUF_SIZE 1024
#define BROADCAST_IP "255.255.255.255"

char server_ip[INET_ADDRSTRLEN];

void discover_server() {
    int udp_sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (udp_sock < 0) {
        perror("UDP socket creation failed");
        exit(EXIT_FAILURE);
    }

    int broadcast = 1;
    if (setsockopt(udp_sock, SOL_SOCKET, SO_BROADCAST, &broadcast, sizeof(broadcast)) < 0) {
        perror("Setting broadcast option failed");
        close(udp_sock);
        exit(EXIT_FAILURE);
    }

    struct sockaddr_in bcast_addr;
    bcast_addr.sin_family = AF_INET;
    bcast_addr.sin_port = htons(PORT);
    bcast_addr.sin_addr.s_addr = inet_addr(BROADCAST_IP);

    const char *msg = "DISCOVER_SERVER";
    if (sendto(udp_sock, msg, strlen(msg), 0,
               (struct sockaddr *)&bcast_addr, sizeof(bcast_addr)) < 0) {
        perror("Sendto failed");
        close(udp_sock);
        exit(EXIT_FAILURE);
    }

    struct sockaddr_in recv_addr;
    socklen_t len = sizeof(recv_addr);
    char response[100];
    int n = recvfrom(udp_sock, response, sizeof(response) - 1, 0,
                     (struct sockaddr *)&recv_addr, &len);
    if (n > 0) {
        response[n] = '\0';
        if (strcmp(response, "SERVER_HERE") == 0) {
            inet_ntop(AF_INET, &recv_addr.sin_addr, server_ip, sizeof(server_ip));
            printf("Server found at %s\n", server_ip);
        }
    } else {
        perror("No response received");
        close(udp_sock);
        exit(EXIT_FAILURE);
    }

    close(udp_sock);
}

int main() {
    discover_server();

    int sock = socket(AF_INET, SOCK_STREAM, 0);
    if (sock < 0) {
        perror("TCP socket creation failed");
        exit(EXIT_FAILURE);
    }

    struct sockaddr_in serv_addr;
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_port = htons(PORT);
    if (inet_pton(AF_INET, server_ip, &serv_addr.sin_addr) <= 0) {
        perror("Invalid server IP address");
        close(sock);
        exit(EXIT_FAILURE);
    }

    if (connect(sock, (struct sockaddr *)&serv_addr, sizeof(serv_addr)) < 0) {
        perror("Connect failed");
        close(sock);
        exit(EXIT_FAILURE);
    }

    printf("Connected to server\n");

    char msg[BUF_SIZE];
    while (fgets(msg, BUF_SIZE, stdin)) {
        send(sock, msg, strlen(msg), 0);
    }

    close(sock);
    return 0;
}
