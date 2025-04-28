#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <pthread.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/select.h>

#define PORT 8889
#define MAX_CLIENTS 5
#define BUF_SIZE 1024

void *handle_client(void *arg) {
    int client_sock = *(int *)arg;
    free(arg);
    char buffer[BUF_SIZE];

    while (1) {
        int n = recv(client_sock, buffer, BUF_SIZE - 1, 0);
        if (n <= 0) {
            break;
        }
        buffer[n] = '\0';
        printf("[Client %lu] %s", (unsigned long)pthread_self(), buffer);
    }

    close(client_sock);
    return NULL;
}

int main() {
    int tcp_sock = socket(AF_INET, SOCK_STREAM, 0);
    if (tcp_sock < 0) {
        perror("TCP socket creation failed");
        exit(EXIT_FAILURE);
    }

    int opt = 1;
    setsockopt(tcp_sock, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    struct sockaddr_in addr;
    addr.sin_family = AF_INET;
    addr.sin_port = htons(PORT);
    addr.sin_addr.s_addr = INADDR_ANY;

    if (bind(tcp_sock, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("TCP bind failed");
        exit(EXIT_FAILURE);
    }

    if (listen(tcp_sock, MAX_CLIENTS) < 0) {
        perror("Listen failed");
        exit(EXIT_FAILURE);
    }

    int udp_sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (udp_sock < 0) {
        perror("UDP socket creation failed");
        exit(EXIT_FAILURE);
    }

    if (bind(udp_sock, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("UDP bind failed");
        exit(EXIT_FAILURE);
    }

    fd_set read_fds;
    int maxfd = (udp_sock > tcp_sock) ? udp_sock : tcp_sock;

    printf("Server is running...\n");

    while (1) {
        FD_ZERO(&read_fds);
        FD_SET(tcp_sock, &read_fds);
        FD_SET(udp_sock, &read_fds);

        if (select(maxfd + 1, &read_fds, NULL, NULL, NULL) < 0) {
            perror("Select failed");
            break;
        }

        if (FD_ISSET(tcp_sock, &read_fds)) {
            int client_sock = accept(tcp_sock, NULL, NULL);
            if (client_sock < 0) {
                perror("Accept failed");
                continue;
            }
            int *pclient = malloc(sizeof(int));
            *pclient = client_sock;
            pthread_t tid;
            pthread_create(&tid, NULL, handle_client, pclient);
            pthread_detach(tid);
        }

        if (FD_ISSET(udp_sock, &read_fds)) {
            char msg[100];
            struct sockaddr_in client_addr;
            socklen_t len = sizeof(client_addr);

            int n = recvfrom(udp_sock, msg, sizeof(msg) - 1, 0,
                             (struct sockaddr *)&client_addr, &len);
            if (n > 0) {
                msg[n] = '\0';
                if (strcmp(msg, "DISCOVER_SERVER") == 0) {
                    char response[] = "SERVER_HERE";
                    sendto(udp_sock, response, strlen(response), 0,
                           (struct sockaddr *)&client_addr, len);
                    printf("Discovery request received, responded to client.\n\n");
                }
            }
        }
    }

    close(tcp_sock);
    close(udp_sock);
    return 0;
}
