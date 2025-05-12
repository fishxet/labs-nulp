import java.util.List;
import java.util.Scanner;

public class lab4 {
    private static Repository repository = new FileRepository("vacancies.dat");
    private static Scanner scanner = new Scanner(System.in);
    public static void main(String[] args) {
        while (true) {
            System.out.println("\nМеню:");
            System.out.println("1. Переглянути всі вакансії");
            System.out.println("2. Додати вакансію");
            System.out.println("3. Видалити вакансію");
            System.out.println("4. Пошук за мовою та зарплатою");
            System.out.println("5. Вакансії компанії");
            System.out.println("0. Вийти");
            System.out.print("Виберіть опцію: ");
            int choice = scanner.nextInt();
            scanner.nextLine();

            switch (choice) {
                case 1:
                    printAllVacancies();
                    break;
                case 2:
                    addVacancy();
                    break;
                case 3:
                    deleteVacancy();
                    break;
                case 4:
                    filterByLanguageAndSalary();
                    break;
                case 5:
                    getByCompany();
                    break;
                case 0:
                    System.exit(0);
                default:
                    System.out.println("Невірний вибір!");
            }
        }
    }

    private static void printAllVacancies() {
        List<Vacancy> vacancies = repository.getAll();
        vacancies.forEach(System.out::println);
    }

    private static void addVacancy() {
        try {
            System.out.print("Назва компанії: ");
            String company = scanner.nextLine();
            System.out.print("Позиція: ");
            String position = scanner.nextLine();
            System.out.print("Мова програмування: ");
            String language = scanner.nextLine();
            System.out.print("Вимоги: ");
            String requirements = scanner.nextLine();
            System.out.print("Зарплата (наприклад, 16000 або 16000$): ");
            String salaryInput = scanner.nextLine().replaceAll("[^\\d.]", ""); // Видаляємо всі нечислові символи
            double salary = Double.parseDouble(salaryInput);

            Vacancy vacancy = new Vacancy(company, position, language, requirements, salary);
            repository.addVacancy(vacancy);
            System.out.println("Вакансію додано!");
        } catch (NumberFormatException e) {
            System.out.println("Помилка: некоректний формат зарплати. Введіть число (наприклад, 16000)!");
        } catch (Exception e) {
            System.out.println("Невідома помилка: " + e.getMessage());
        }
    }

    private static void deleteVacancy() {
        System.out.print("Введіть ID вакансії для видалення: ");
        int id = scanner.nextInt();
        if (repository.deleteVacancy(id)) {
            System.out.println("Вакансію видалено!");
        } else {
            System.out.println("Вакансія не знайдена.");
        }
    }

    private static void filterByLanguageAndSalary() {
        System.out.print("Мова: ");
        String language = scanner.nextLine();
        System.out.print("Мінімальна зарплата: ");
        double min = scanner.nextDouble();
        System.out.print("Максимальна зарплата: ");
        double max = scanner.nextDouble();
        scanner.nextLine();

        List<Vacancy> result = repository.filterByLanguageAndSalary(language, min, max);
        result.forEach(System.out::println);
    }

    private static void getByCompany() {
        System.out.print("Назва компанії: ");
        String company = scanner.nextLine();
        List<Vacancy> result = repository.getByCompany(company);
        result.forEach(System.out::println);
    }
}