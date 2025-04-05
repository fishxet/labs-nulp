#include <iostream>
#include <unistd.h>
#include <sys/wait.h>
#include <semaphore.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <csignal>
#include <sys/select.h>

sem_t* unique_sem = nullptr;
sem_t* limiter_sem = nullptr;
sem_t* shared_mutex = nullptr;
int num_proc = 10;
void* shm_ptr = MAP_FAILED;

const std::string UNIQUE_SEM_NAME = "/unique_" + std::to_string(getpid());

void cleanup() {
    if (unique_sem) {
        sem_close(unique_sem);
        sem_unlink(UNIQUE_SEM_NAME.c_str());
    }
    if (limiter_sem) {
        sem_close(limiter_sem);
        sem_unlink("/process_limiter");
    }
    if (shm_ptr != MAP_FAILED) {
        munmap(shm_ptr, sizeof(sem_t));
        shm_unlink("/mutex_shm");
    }
}

void signal_handler(int) {
    cleanup();
    exit(1);
}

void wait_with_timeout(int seconds) {
    struct timeval tv;
    tv.tv_sec = seconds;
    tv.tv_usec = 0;
    select(0, nullptr, nullptr, nullptr, &tv);
}

int main() {
    signal(SIGINT, signal_handler);
    signal(SIGTERM, signal_handler);
    int status;
    pid_t child_pid;
    unique_sem = sem_open(UNIQUE_SEM_NAME.c_str(), O_CREAT | O_EXCL, 0644, 1);
    if (unique_sem == SEM_FAILED) {
        if (errno == EEXIST) {
            std::cerr << "Програма вже запущена!\n";
            return 1;
        }
        perror("Помилка семафора");
        return 1;
    }
    int shm_fd = shm_open("/mutex_shm", O_CREAT | O_RDWR, 0666);
    ftruncate(shm_fd, sizeof(sem_t));
    shm_ptr = mmap(nullptr, sizeof(sem_t), PROT_READ | PROT_WRITE, MAP_SHARED, shm_fd, 0);
    shared_mutex = static_cast<sem_t*>(shm_ptr);
    shared_mutex = sem_open("/shared_mutex", O_CREAT, 0644, 1);
    if (shared_mutex == SEM_FAILED) {
        perror("Помилка м'ютексу");
        cleanup();
        return 1;
    }
    limiter_sem = sem_open("/process_limiter", O_CREAT, 0644, 3);
    if (limiter_sem == SEM_FAILED) {
        perror("Помилка обмежувача");
        cleanup();
        return 1;
    }
    std::cout << "Створення " << num_proc << " процесів...\n";
    for (int i = 0; i < num_proc; ++i) {
        pid_t pid = fork();
        if (pid == 0) {
            sem_wait(limiter_sem);
            
            sem_wait(shared_mutex);
            std::cout << "Процес " << i+1 
                      << " (PID: " << getpid() << ") стартував\n";
            sem_post(shared_mutex);
            
            usleep(100000);
            sem_post(limiter_sem);
            
            sem_wait(shared_mutex); 
            std::cout << "Процес " << i+1 
                      << " (PID: " << getpid() << ") завершив роботу\n";
            sem_post(shared_mutex);
            
            _exit(0);
        } 
        else if (pid < 0) {
            perror("Помилка fork");
            sem_post(limiter_sem);
        }
    }
    std::cout << "Очікування 5 секунд...\n";
    wait_with_timeout(5);
    std::cout << "Таймер спрацював!\n";
    std::cout << "Очікування завершення роботи дочірніх процесів...\n";
    while ((child_pid = wait(&status))) {
        if (child_pid == -1) {
            if (errno == ECHILD) {
                break;
            }
            perror("Помилка очікування");
            continue;
        }
        
        sem_wait(shared_mutex);
        std::cout << "Дочірній процес " << child_pid 
                  << " завершився зі статусом: " 
                  << WEXITSTATUS(status) << "\n";
        sem_post(shared_mutex);
    }

    cleanup();
    std::cout << "Завершення роботи.\n";
    return 0;
}