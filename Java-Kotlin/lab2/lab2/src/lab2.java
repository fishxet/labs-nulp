import java.util.Scanner;
import java.util.InputMismatchException;
import java.util.Locale;
public class lab2 {
    public static void LessNumber() {
        Scanner scanner = new Scanner(System.in);
        int[] nums = new int[3];
        String[] labels = {"перше", "друге", "третє"};

        for (int i = 0; i < 3; ) {
            try {
                System.out.printf("Введіть %s ціле число: ", labels[i]);
                nums[i] = Integer.parseInt(scanner.nextLine());
                i++;
            } catch (NumberFormatException e) {
                System.out.println("Помилка: введіть ціле число.");
            }
        }

        System.out.println("Найменше число: " + Math.min(nums[0], Math.min(nums[1], nums[2])));
    }
    public static void InequalitySolver() {
        Scanner scanner = new Scanner(System.in);
        scanner.useLocale(Locale.US); // Крапка замість коми
        double a = 0, b = 0, c = 0;
        boolean isValid = false;

        // Цикл перевірки вводу
        while (!isValid) {
            try {
                System.out.print("Введіть a, b, c (через пробіл): ");
                a = scanner.nextDouble();
                b = scanner.nextDouble();
                c = scanner.nextDouble();
                isValid = true; // Вихід з циклу
            } catch (InputMismatchException e) {
                System.out.println("Помилка: введіть числа у форматі 0.0 (наприклад: -7.6 3.4 11.3)");
                scanner.nextLine(); // Очистка буфера
            }
        }

        scanner.close();

        // Обчислення дискримінанта
        double discriminant = (b * b) - (4 * a * c);

        if (discriminant < 0) {

            System.out.printf("x ∈ (-∞; %.2f)%n", a);
        } else if (discriminant == 0) {
            double x0 = -b / (2 * a);
            if (x0 == a) {
                System.out.println("Розв’язок: ∅ (знаменник дорівнює нулю)");
            } else if (a < x0) {
                System.out.printf("x ∈ (-∞; %.2f)%n", a);
            } else {
                System.out.printf("x ∈ (-∞; %.2f) ∪ (%.2f; +∞)%n", x0, x0);
            }
        } else {
            double x1 = (-b - Math.sqrt(discriminant)) / (2 * a);
            double x2 = (-b + Math.sqrt(discriminant)) / (2 * a);
            if (x1 > x2) { // Перевірка порядку коренів
                double temp = x1;
                x1 = x2;
                x2 = temp;
            }

            if (a < x1) {
                System.out.printf("x ∈ (-∞; %.2f) ∪ (%.2f; +∞)%n", a, x2);
            } else if (a > x2) {
                System.out.printf("x ∈ (-∞; %.2f) ∪ (%.2f; +∞)%n", x1, x2);
            } else {
                System.out.printf("x ∈ (-∞; %.2f) ∪ (%.2f; %.2f)%n", x1, a, x2);
            }
        }

    }
    public static void main(String[] args) {
        LessNumber();
        InequalitySolver();
    }
}
