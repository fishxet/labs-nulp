#include <iostream>
#include <fstream>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <vector>

const int MAX_NUM = 100;
const std::string OUTPUT_FILE = "output.txt";

// Спільні ресурси
std::mutex file_mutex;
std::mutex event_mutex;
std::condition_variable event_cv;
int active_threads = 0;
const int MAX_ACTIVE_THREADS = 2;

std::ofstream output;

void manage_critical_section(bool acquire) {
    std::unique_lock<std::mutex> lock(event_mutex);
    if (acquire) {
        while (active_threads >= MAX_ACTIVE_THREADS) {
            event_cv.wait(lock);
        }
        active_threads++;
    } else {
        active_threads--;
        event_cv.notify_one();
    }
}

void positive_numbers(int pair_id, bool use_event, bool use_critical) {
    if (use_event) {
        std::unique_lock<std::mutex> lock(event_mutex);
        event_cv.wait(lock);
    }

    if (use_critical) {
        manage_critical_section(true);
    }

    for (int i = 1; i <= MAX_NUM; ++i) {
        std::lock_guard<std::mutex> lock(file_mutex);
        output << "Pair " << pair_id << " (Positive): " << i << std::endl;
    }

    if (use_critical) {
        manage_critical_section(false);
    }
}

void negative_numbers(int pair_id, bool use_event, bool use_critical) {
    if (use_event) {
        std::unique_lock<std::mutex> lock(event_mutex);
        event_cv.wait(lock);
    }

    if (use_critical) {
        manage_critical_section(true);
    }

    for (int i = -1; i >= -MAX_NUM; --i) {
        std::lock_guard<std::mutex> lock(file_mutex);
        output << "Pair " << pair_id << " (Negative): " << i << std::endl;
    }

    if (use_critical) {
        manage_critical_section(false);
    }
}

int main() {
    output.open(OUTPUT_FILE);
    if (!output.is_open()) {
        std::cerr << "Помилка відкриття файлу!" << std::endl;
        return 1;
    }

    std::vector<std::thread> threads;

    // Запуск потоків
    threads.emplace_back(positive_numbers, 1, false, false);
    threads.emplace_back(negative_numbers, 1, false, false);
    threads.emplace_back(positive_numbers, 2, true, false);
    threads.emplace_back(negative_numbers, 2, true, false);
    threads.emplace_back(positive_numbers, 3, false, true);
    threads.emplace_back(negative_numbers, 3, false, true);

    std::this_thread::sleep_for(std::chrono::seconds(1));
    {
        std::lock_guard<std::mutex> lock(event_mutex);
        event_cv.notify_all();
    }

    for (std::thread& t : threads) {
        t.join();
    }

    output.close();
    return 0;
}