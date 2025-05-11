import java.util.*
import java.util.function.Consumer

object lab4kot {
    private val repository: Repository = FileRepository("vacancies.dat")
    private val scanner = Scanner(System.`in`)

    @JvmStatic
    fun main(args: Array<String>) {
        while (true) {
            println("\nМеню:")
            println("1. Переглянути всі вакансії")
            println("2. Додати вакансію")
            println("3. Видалити вакансію")
            println("4. Пошук за мовою та зарплатою")
            println("5. Вакансії компанії")
            println("0. Вийти")
            print("Виберіть опцію: ")
            val choice = scanner.nextInt()
            scanner.nextLine() // Очистка буфера

            when (choice) {
                1 -> printAllVacancies()
                2 -> addVacancy()
                3 -> deleteVacancy()
                4 -> filterByLanguageAndSalary()
                5 -> byCompany
                0 -> {
                    System.exit(0)
                    println("Невірний вибір!")
                }

                else -> println("Невірний вибір!")
            }
        }
    }

    private fun printAllVacancies() {
        val vacancies = repository.all
        vacancies.forEach(Consumer { x: Vacancy? -> println(x) })
    }

    private fun addVacancy() {
        try {
            print("Назва компанії: ")
            val company = scanner.nextLine()
            print("Позиція: ")
            val position = scanner.nextLine()
            print("Мова програмування: ")
            val language = scanner.nextLine()
            print("Вимоги: ")
            val requirements = scanner.nextLine()
            print("Зарплата (наприклад, 16000 або 16000$): ")
            val salaryInput = scanner.nextLine().replace("[^\\d.]".toRegex(), "") // Видаляємо всі нечислові символи
            val salary = salaryInput.toDouble()

            val vacancy = Vacancy(company, position, language, requirements, salary)
            repository.addVacancy(vacancy)
            println("Вакансію додано!")
        } catch (e: NumberFormatException) {
            println("Помилка: некоректний формат зарплати. Введіть число (наприклад, 16000)!")
        } catch (e: Exception) {
            println("Невідома помилка: " + e.message)
        }
    }

    private fun deleteVacancy() {
        print("Введіть ID вакансії для видалення: ")
        val id = scanner.nextInt()
        if (repository.deleteVacancy(id)) {
            println("Вакансію видалено!")
        } else {
            println("Вакансія не знайдена.")
        }
    }

    private fun filterByLanguageAndSalary() {
        print("Мова: ")
        val language = scanner.nextLine()
        print("Мінімальна зарплата: ")
        val min = scanner.nextDouble()
        print("Максимальна зарплата: ")
        val max = scanner.nextDouble()
        scanner.nextLine()

        val result = repository.filterByLanguageAndSalary(language, min, max)
        result.forEach(Consumer { x: Vacancy? -> println(x) })
    }

    private val byCompany: Unit
        get() {
            print("Назва компанії: ")
            val company = scanner.nextLine()
            val result = repository.getByCompany(company)
            result.forEach(Consumer { x: Vacancy? -> println(x) })
        }
}