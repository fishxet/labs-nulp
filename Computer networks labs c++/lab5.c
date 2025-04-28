#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <semaphore.h>
#include <unistd.h>

sem_t sem;
FILE *log_file;

void* P1_func(void* arg) {
    while (1) {
        sem_wait(&sem);
        fprintf(log_file, "[P1] in CS\n");
        fflush(log_file);
        // імітація роботи в критичній секції
        usleep(10000); // 10 мс
        sem_post(&sem);
        usleep(1000); // 1 мс між заходами
    }
    return NULL;
}

void* P2_func(void* arg) {
    while (1) {
        if (sem_trywait(&sem) == 0) {
            fprintf(log_file, "[P2] in CS\n");
            fflush(log_file);
            // робота в критичній секції
            usleep(5000); // 5 мс
            sem_post(&sem);
        } else {
            fprintf(log_file, "    [P2] starved!\n");
            fflush(log_file);
            usleep(1000); // 1 мс перед наступною спробою
        }
    }
    return NULL;
}

int main() {
    pthread_t p1, p2;

    // Ініціалізація семафору
    sem_init(&sem, 0, 1);

    // Відкриваємо файл для логування
    log_file = fopen("log.txt", "w");
    if (!log_file) {
        perror("Не вдалося відкрити файл");
        exit(EXIT_FAILURE);
    }

    // Створення потоків
    pthread_create(&p1, NULL, P1_func, NULL);
    pthread_create(&p2, NULL, P2_func, NULL);

    // Чекаємо певний час роботи, наприклад, 2 секунди
    sleep(2);

    // Завершення програми
    fclose(log_file);
    sem_destroy(&sem);
    exit(EXIT_SUCCESS);
}
