#include <iostream>
#include <pthread.h>
#include <unistd.h>
#include <cstdlib>

pthread_mutex_t criticalSection;
pthread_mutex_t syncSection;
pthread_cond_t turnCond = PTHREAD_COND_INITIALIZER;

int counterPositive = 1;
int counterNegative = -1;

bool isPositiveTurn = false;

void* PrintPositiveNumbers(void* arg) {
    int threadId = *(int*)arg;
    while (counterPositive <= 500) {
        if (rand() % 2 == 0) {
            std::cout << counterPositive << " ";
            counterPositive++;
        }
        usleep(100);  // 100 мікросекунд
    }
    std::cout << "\n \t Thread " << threadId << " finished working.\n";
    return nullptr;
}

void* PrintNegativeNumbers(void* arg) {
    int threadId = *(int*)arg;
    while (counterNegative >= -500) {
        if (rand() % 2 == 0) {
            std::cout << counterNegative << " ";
            counterNegative--;
        }
        usleep(100);
    }
    std::cout << "\n \t Thread " << threadId << " finished working.\n";
    return nullptr;
}

void* CriticalSectionPositive(void* arg) {
    int threadId = *(int*)arg;
    pthread_mutex_lock(&criticalSection);
    for (int i = 1; i <= 500; i++) {
        std::cout << i << " ";
        usleep(100);
    }
    pthread_mutex_unlock(&criticalSection);
    std::cout << "\n \t Thread " << threadId << " finished working.\n";
    return nullptr;
}

void* CriticalSectionNegative(void* arg) {
    int threadId = *(int*)arg;
    pthread_mutex_lock(&criticalSection);
    for (int i = -1; i >= -500; i--) {
        std::cout << i << " ";
        usleep(100);
    }
    pthread_mutex_unlock(&criticalSection);
    std::cout << "\n \t Thread " << threadId << " finished working.\n";
    return nullptr;
}

void* OneEventPositive(void* arg) {
    int threadId = *(int*)arg;
    for (int i = 1; i <= 500; i++) {
        pthread_mutex_lock(&syncSection);
        while (!isPositiveTurn) {
            pthread_cond_wait(&turnCond, &syncSection);
        }
        std::cout << i << " ";
        isPositiveTurn = false;
        pthread_cond_signal(&turnCond);
        pthread_mutex_unlock(&syncSection);
        usleep(100);
    }
    std::cout << "\n \t Thread " << threadId << " finished working.\n";
    return nullptr;
}

void* OneEventNegative(void* arg) {
    int threadId = *(int*)arg;
    for (int i = -1; i >= -500; i--) {
        pthread_mutex_lock(&syncSection);
        while (isPositiveTurn) {
            pthread_cond_wait(&turnCond, &syncSection);
        }
        std::cout << i << " ";
        isPositiveTurn = true;
        pthread_cond_signal(&turnCond);
        pthread_mutex_unlock(&syncSection);
        usleep(100);
    }
    std::cout << "\n \t Thread " << threadId << " finished working.\n";
    return nullptr;
}

int main() {
    srand(time(nullptr));

    std::cout << "--- Threads 1 and 2 started working ---\n";

    pthread_t threads[6];
    int threadIds[6] = {1, 2, 3, 4, 5, 6};

    // Без синхронізації
    pthread_create(&threads[0], nullptr, PrintPositiveNumbers, &threadIds[0]);
    pthread_create(&threads[1], nullptr, PrintNegativeNumbers, &threadIds[1]);
    pthread_join(threads[0], nullptr);
    pthread_join(threads[1], nullptr);

    std::cout << "\n \t --- Threads 3 and 4 started working (Event sync) ---\n";

    pthread_mutex_init(&syncSection, nullptr);
    isPositiveTurn = false;

    pthread_create(&threads[2], nullptr, OneEventPositive, &threadIds[2]);
    pthread_create(&threads[3], nullptr, OneEventNegative, &threadIds[3]);
    pthread_join(threads[2], nullptr);
    pthread_join(threads[3], nullptr);

    std::cout << "\n \t--- Threads 5 and 6 started working (Critical section) ---\n";

    pthread_mutex_init(&criticalSection, nullptr);

    pthread_create(&threads[4], nullptr, CriticalSectionPositive, &threadIds[4]);
    pthread_create(&threads[5], nullptr, CriticalSectionNegative, &threadIds[5]);
    pthread_join(threads[4], nullptr);
    pthread_join(threads[5], nullptr);

    pthread_mutex_destroy(&criticalSection);
    pthread_mutex_destroy(&syncSection);

    std::cout << "\n\tEnd\n";
    return 0;
}
