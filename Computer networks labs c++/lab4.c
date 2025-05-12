#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <unistd.h>
#include <string.h>

// Виробник–споживач (Finish–Start) для обчислення коефіцієнтів многочлена C = A * B
// Варіант 14: продуценти обчислюють a_i * b_j, споживач агрегує в c[k]

// Структура повідомлення між потоками
struct msg {
    int k;       // індекс c[k]
    double val;  // вклад a_i * b_j
};

int *N, *M;           // порядки многочленів (кількість старших степенів)
double *a, *b, *c;     // масиви коефіцієнтів A, B та результату C
int fd[2];             // pipe fd[0] для читання, fd[1] для запису
pthread_mutex_t pipe_mutex;

void *producer(void *arg) {
    int idx = *(int*)arg;
    free(arg);
    int m = *M;
    int i = idx / (m + 1);
    int j = idx % (m + 1);
    double prod = a[i] * b[j];
    struct msg mmsg = { .k = i + j, .val = prod };

    pthread_mutex_lock(&pipe_mutex);
    write(fd[1], &mmsg, sizeof(mmsg));
    pthread_mutex_unlock(&pipe_mutex);
    return NULL;
}

void *consumer(void *arg) {
    (void)arg;
    struct msg mmsg;
    // Читати доти, доки є повідомлення
    while (read(fd[0], &mmsg, sizeof(mmsg)) == sizeof(mmsg)) {
        c[mmsg.k] += mmsg.val;
    }
    return NULL;
}

int main() {
    // Читання вхідних даних
    printf("Введіть порядок A (n) та B (m): ");
    int n, m;
    if (scanf("%d %d", &n, &m) != 2) {
        fprintf(stderr, "Помилка введення порядків.\n");
        return EXIT_FAILURE;
    }
    N = &n;
    M = &m;
    // Виділення пам'яті для коефіцієнтів
    a = malloc((n+1) * sizeof(double));
    b = malloc((m+1) * sizeof(double));
    c = calloc(n + m + 1, sizeof(double));
    if (!a || !b || !c) {
        perror("malloc");
        return EXIT_FAILURE;
    }
    printf("Введіть %d коефіцієнтів A(x) (від a0 до a%d):\n", n+1, n);
    for (int i = 0; i <= n; i++) scanf("%lf", &a[i]);
    printf("Введіть %d коефіцієнтів B(x) (від b0 до b%d):\n", m+1, m);
    for (int j = 0; j <= m; j++) scanf("%lf", &b[j]);

    // Ініціалізація pipe та м'ютекса
    if (pipe(fd) == -1) { perror("pipe"); exit(EXIT_FAILURE); }
    pthread_mutex_init(&pipe_mutex, NULL);

    int P = (n+1) * (m+1);
    pthread_t *producers = malloc(P * sizeof(pthread_t));
    pthread_t consumer_thread;

    // Створення threads-продуцентів
    for (int idx = 0; idx < P; idx++) {
        int *p = malloc(sizeof(int));
        *p = idx;
        if (pthread_create(&producers[idx], NULL, producer, p) != 0) {
            perror("pthread_create producer");
            return EXIT_FAILURE;
        }
    }

    // Очікуємо завершення всіх продуцентів (Finish)
    for (int i = 0; i < P; i++) pthread_join(producers[i], NULL);
    // Закриваємо кінець запису, щоб потік споживача бачив EOF
    close(fd[1]);

    // Створюємо потік споживача (Start після Finish)
    if (pthread_create(&consumer_thread, NULL, consumer, NULL) != 0) {
        perror("pthread_create consumer");
        return EXIT_FAILURE;
    }
    pthread_join(consumer_thread, NULL);

    // Вивід результату
    printf("\nРезультуючі коефіцієнти C(x)=A(x)*B(x):\n");
    for (int k = 0; k <= n + m; k++) {
        printf("c[%d] = %.6f\n", k, c[k]);
    }

    // Очищення ресурсів
    pthread_mutex_destroy(&pipe_mutex);
    free(a); free(b); free(c); free(producers);
    close(fd[0]);
    return 0;
}
