#define _XOPEN_SOURCE 700
#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <math.h>

#define MAX_N 10

typedef struct {
    int    n;
    double a[MAX_N][MAX_N+1];
} mat_data_t;

static mat_data_t mat;

// Власний бар’єр
typedef struct {
    pthread_mutex_t mutex;
    pthread_cond_t  cond;
    int             count;
    int             total;
    int             generation;
} barrier_t;

static barrier_t barrier;

static void barrier_init(barrier_t *b, int total_threads) {
    pthread_mutex_init(&b->mutex, NULL);
    pthread_cond_init(&b->cond, NULL);
    b->total      = total_threads;
    b->count      = total_threads;
    b->generation = 0;
}

static void barrier_wait(barrier_t *b) {
    pthread_mutex_lock(&b->mutex);
    int gen = b->generation;

    if (--b->count == 0) {
        // Останній потік доходить – піднімаємо бар’єр
        b->generation++;
        b->count = b->total;
        pthread_cond_broadcast(&b->cond);
    } else {
        // Інші чекають
        while (gen == b->generation)
            pthread_cond_wait(&b->cond, &b->mutex);
    }

    pthread_mutex_unlock(&b->mutex);
}

static void barrier_destroy(barrier_t *b) {
    pthread_mutex_destroy(&b->mutex);
    pthread_cond_destroy(&b->cond);
}

// Обмін двох змінних
static void swap(double *x, double *y) {
    double t = *x; *x = *y; *y = t;
}

static void* worker(void* arg) {
    int tid = *(int*)arg;   // 0 або 1
    int n   = mat.n;

    // --- ПРЯМИЙ ХІД ---
    for (int k = 0; k < n; ++k) {
        if (tid == 0) {
            // 1) Вибір головного елемента та обмін рядків
            int max_r = k;
            for (int i = k+1; i < n; ++i)
                if (fabs(mat.a[i][k]) > fabs(mat.a[max_r][k]))
                    max_r = i;
            if (fabs(mat.a[max_r][k]) < 1e-12) {
                fprintf(stderr, "Вироджена система або немає єдиного рішення\n");
                exit(EXIT_FAILURE);
            }
            if (max_r != k)
                for (int j = k; j <= n; ++j)
                    swap(&mat.a[k][j], &mat.a[max_r][j]);
        }

        // --- ПЕРШИЙ БАР’ЄР: start–start ---
        barrier_wait(&barrier);

        // 2) Паралельна обробка своїх рядків
        for (int i = k+1+tid; i < n; i += 2) {
            double f = mat.a[i][k] / mat.a[k][k];
            for (int j = k; j <= n; ++j)
                mat.a[i][j] -= f * mat.a[k][j];
        }

        // --- ДРУГИЙ БАР’ЄР: синхронізація перед наступним k ---
        barrier_wait(&barrier);
    }

    // --- ОБЕРНЕНИЙ ХІД (лише потік 0) ---
    if (tid == 0) {
        double x[MAX_N];
        for (int i = n-1; i >= 0; --i) {
            double sum = mat.a[i][n];
            for (int j = i+1; j < n; ++j)
                sum -= mat.a[i][j] * x[j];
            x[i] = sum / mat.a[i][i];
        }
        // Вивід рішення
        printf("\n=== Розв’язок СЛАР (n=%d) ===\n", n);
        for (int i = 0; i < n; ++i)
            printf("x[%d] = %.6f\n", i, x[i]);
        printf("=============================\n");
    }

    return NULL;
}

int main(void) {
    pthread_t thr[2];
    int       tid[2] = {0, 1};

    // Ввід системи
    printf("Введіть n (1..%d): ", MAX_N);
    if (scanf("%d", &mat.n) != 1 || mat.n < 1 || mat.n > MAX_N) {
        fprintf(stderr, "Некоректне n!\n");
        return 1;
    }
    printf("Введіть %d рядків по %d чисел:\n", mat.n, mat.n+1);
    for (int i = 0; i < mat.n; ++i)
        for (int j = 0; j <= mat.n; ++j)
            if (scanf("%lf", &mat.a[i][j]) != 1) {
                fprintf(stderr, "Помилка вводу!\n");
                return 1;
            }

    // Ініціалізація власного бар’єра для 2 потоків
    barrier_init(&barrier, 2);

    // Створення воркерів
    for (int i = 0; i < 2; ++i)
        pthread_create(&thr[i], NULL, worker, &tid[i]);

    // Очікуємо завершення обох потоків
    for (int i = 0; i < 2; ++i)
        pthread_join(thr[i], NULL);

    // Звільнення ресурсів бар’єра
    barrier_destroy(&barrier);

    return 0;
}
