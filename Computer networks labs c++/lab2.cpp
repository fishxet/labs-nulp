#include <iostream>
#include <fstream>
#include <pthread.h>    // Бібліотека для роботи з потоками POSIX
#include <vector>
#include <thread>       // Для std::this_thread
#include <chrono>       // Для роботи з часом

// Константи програми
const int MAX_NUM = 500;                // Максимальне число (додатнє/від'ємне)
const std::string OUTPUT_FILE = "output.txt";  // Ім'я вихідного файлу

// Структура для передачі параметрів у потік
struct ThreadParams {
    std::ofstream* output;   // Вказівник на файловий потік для запису
    int pair_id;             // Ідентифікатор пари потоків
    bool is_positive;        // Чи виводити додатні числа (true - додатні, false - від'ємні)
    bool use_event;          // Чи чекати на подію перед початком роботи
    bool use_critical;       // Чи використовувати критичну секцію
};

// Глобальні змінні для синхронізації (ініціалізуються стандартними значеннями)
pthread_mutex_t file_mutex = PTHREAD_MUTEX_INITIALIZER;      // М'ютекс для захисту запису у файл
pthread_mutex_t event_mutex = PTHREAD_MUTEX_INITIALIZER;     // М'ютекс для подій
pthread_cond_t event_cv = PTHREAD_COND_INITIALIZER;          // Умовна змінна для подій
pthread_mutex_t critical_mutex = PTHREAD_MUTEX_INITIALIZER;  // М'ютекс для критичної секції
pthread_cond_t critical_cv = PTHREAD_COND_INITIALIZER;       // Умовна змінна для критичної секції
int active_threads = 0;                  // Лічильник активних потоків у критичній секції
const int MAX_ACTIVE_THREADS = 2;        // Максимум потоків у критичній секції
bool event_triggered = false;            // Прапорець події

// Функція для керування критичною секцією
void manage_critical_section(bool acquire) {
    // Захоплюємо м'ютекс критичної секції
    pthread_mutex_lock(&critical_mutex);
    
    if (acquire) {
        // Якщо потрібно увійти в критичну секцію
        while (active_threads >= MAX_ACTIVE_THREADS) {
            // Чекаємо, поки кількість потоків у секції не зменшиться
            pthread_cond_wait(&critical_cv, &critical_mutex);
        }
        active_threads++;  // Збільшуємо лічильник активних потоків
    } else {
        // Якщо потрібно вийти з критичної секції
        active_threads--;  // Зменшуємо лічильник
        // Сповіщаємо один з очікуючих потоків
        pthread_cond_signal(&critical_cv);
    }
    
    // Звільняємо м'ютекс
    pthread_mutex_unlock(&critical_mutex);
}

// Основна функція потоку
void* number_writer(void* arg) {
    // Отримуємо параметри потоку
    ThreadParams* params = (ThreadParams*)arg;
    
    // Обробка події (якщо потрібно)
    if (params->use_event) {
        pthread_mutex_lock(&event_mutex);  // Захоплюємо м'ютекс подій
        while (!event_triggered) {
            // Чекаємо на подію
            pthread_cond_wait(&event_cv, &event_mutex);
        }
        pthread_mutex_unlock(&event_mutex);  // Звільняємо м'ютекс
    }

    // Обробка критичної секції (якщо потрібно)
    if (params->use_critical) {
        manage_critical_section(true);  // Вхід у критичну секцію
    }

    // Налаштування параметрів циклу в залежності від типу чисел
    const int start = params->is_positive ? 1 : -1;
    const int end = params->is_positive ? MAX_NUM : -MAX_NUM;
    const int step = params->is_positive ? 1 : -1;
    const char* type = params->is_positive ? "Positive" : "Negative";

    // Основний цикл запису чисел
    for (int i = start; (params->is_positive ? i <= end : i >= end); i += step) {
        pthread_mutex_lock(&file_mutex);  // Захоплюємо м'ютекс файлу
        // Записуємо число у файл
        *(params->output) << "Pair " << params->pair_id << " (" << type << "): " << i << std::endl;
        pthread_mutex_unlock(&file_mutex);  // Звільняємо м'ютекс
    }

    // Вихід з критичної секції (якщо потрібно)
    if (params->use_critical) {
        manage_critical_section(false);
    }

    delete params;  // Видаляємо параметри
    return NULL;    // Завершуємо потік
}

int main() {
    // Відкриваємо файл для запису
    std::ofstream output(OUTPUT_FILE.c_str());
    if (!output.is_open()) {
        std::cerr << "Помилка відкриття файлу!" << std::endl;
        return 1;
    }

    // Вектор ідентифікаторів потоків
    std::vector<pthread_t> threads(6);
    
    // Параметри для кожного з 6 потоків:
    // Потік 1: додатні числа, без події, без критичної секції
    ThreadParams* params1 = new ThreadParams();
    params1->output = &output;
    params1->pair_id = 1;
    params1->is_positive = true;
    params1->use_event = false;
    params1->use_critical = false;

    // Потік 2: від'ємні числа, без події, без критичної секції
    ThreadParams* params2 = new ThreadParams();
    params2->output = &output;
    params2->pair_id = 1;
    params2->is_positive = false;
    params2->use_event = false;
    params2->use_critical = false;

    // Потік 3: додатні числа, з очікуванням події, без критичної секції
    ThreadParams* params3 = new ThreadParams();
    params3->output = &output;
    params3->pair_id = 2;
    params3->is_positive = true;
    params3->use_event = true;
    params3->use_critical = false;

    // Потік 4: від'ємні числа, з очікуванням події, без критичної секції
    ThreadParams* params4 = new ThreadParams();
    params4->output = &output;
    params4->pair_id = 2;
    params4->is_positive = false;
    params4->use_event = true;
    params4->use_critical = false;

    // Потік 5: додатні числа, без події, з критичною секцією
    ThreadParams* params5 = new ThreadParams();
    params5->output = &output;
    params5->pair_id = 3;
    params5->is_positive = true;
    params5->use_event = false;
    params5->use_critical = true;

    // Потік 6: від'ємні числа, без події, з критичною секцією
    ThreadParams* params6 = new ThreadParams();
    params6->output = &output;
    params6->pair_id = 3;
    params6->is_positive = false;
    params6->use_event = false;
    params6->use_critical = true;

    // Створення потоків
    pthread_create(&threads[0], NULL, number_writer, params1);
    pthread_create(&threads[1], NULL, number_writer, params2);
    pthread_create(&threads[2], NULL, number_writer, params3);
    pthread_create(&threads[3], NULL, number_writer, params4);
    pthread_create(&threads[4], NULL, number_writer, params5);
    pthread_create(&threads[5], NULL, number_writer, params6);

    // Затримка 100 мс (0.1 секунди) перед сповіщенням
    std::this_thread::sleep_for(std::chrono::nanoseconds(100000000));
    
    // Сповіщення про подію для потоків, які очікують
    pthread_mutex_lock(&event_mutex);
    event_triggered = true;              // Встановлюємо прапорець
    pthread_cond_broadcast(&event_cv);   // Сповіщаємо всі потоки
    pthread_mutex_unlock(&event_mutex);

    // Очікування завершення всіх потоків
    for (size_t i = 0; i < threads.size(); ++i) {
        pthread_join(threads[i], NULL);
    }

    output.close();  // Закриваємо файл
    
    // Знищення об'єктів синхронізації
    pthread_mutex_destroy(&file_mutex);
    pthread_mutex_destroy(&event_mutex);
    pthread_mutex_destroy(&critical_mutex);
    pthread_cond_destroy(&event_cv);
    pthread_cond_destroy(&critical_cv);

    return 0;
}