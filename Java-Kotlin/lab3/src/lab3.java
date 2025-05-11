import java.util.InputMismatchException;
import java.util.Scanner;

public class lab3 {

    public static void main(String[] args) {
        task1();
        task2();
    }

    // Завдання 1: Обчислення arccotg(x)
    public static void task1() {
        Scanner scanner = new Scanner(System.in);
        System.out.println("\n=== Завдання 1: Обчислення arccotg(x) ===");
        double x = 0;

        // Обробка введення з перевірками
        while (true) {
            try {
                System.out.print("Введіть x (x ≠ 0): ");
                x = scanner.nextDouble();
                if (x == 0) throw new IllegalArgumentException();
                break;
            } catch (InputMismatchException e) {
                System.out.println("Помилка: Очікується число!");
                scanner.nextLine();
            } catch (IllegalArgumentException e) {
                System.out.println("Помилка: x не може бути нулем!");
            }
        }

        // Обчислення ряду
        double epsilon = 0.00001;
        double sum = Math.PI / 2;
        double term = 1.0 / (2 * 0 + 1);
        int k = 0;

        while (Math.abs(term) >= epsilon) {
            sum -= Math.pow(-1, k) * term;
            k++;
            term = Math.pow(x, 2 * k) / (2 * k + 1);
        }
        // Вивід результатів
        System.out.println("\n┌───────────────────────────────┐");
        System.out.printf("│%-15s %12.6f │\n", "Ряд:", sum);
        System.out.printf("│%-15s %12.6f │\n", "Бібліотека:", Math.atan(1 / x));
        System.out.printf("│%-15s %12d │\n", "Доданків:", k);
        System.out.println("└───────────────────────────────┘");
    }

    // Завдання 2: Табулювання функції u = x² + y⁴x
        public static void task2() {
            Scanner scanner = new Scanner(System.in);
            System.out.println("\n=== Завдання 2: Табулювання функції u = x² + y⁴x ===");

            // Обробка введення меж
            double a = 0, b = 0, c = 0, d = 0;
            while (true) {
                try {
                    System.out.print("Введіть межі x [a b]: ");
                    a = scanner.nextDouble();
                    b = scanner.nextDouble();
                    System.out.print("Введіть межі y [c d]: ");
                    c = scanner.nextDouble();
                    d = scanner.nextDouble();
                    if (a > b) { double tmp = a; a = b; b = tmp; }
                    if (c > d) { double tmp = c; c = d; d = tmp; }
                    break;
                } catch (InputMismatchException e) {
                    System.out.println("Помилка: Введіть числа!");
                    scanner.nextLine();
                }
            }

            // Обчислення кроків
            double hx = (b - a) / 7;
            double hy = (d - c) / 7;

            // Вивід таблиці з форматуванням
            System.out.println("\n┌───────────" + "┬────────────".repeat(8) + "┐");
            System.out.printf("│ %-9s ", "y\\x");
            for (int i = 0; i < 8; i++) {
                System.out.printf("│ %10.2f ", a + i * hx);
            }
            System.out.println("│\n├───────────" + "┼────────────".repeat(8) + "┤");

            // Тіло таблиці
            for (int i = 0; i < 8; i++) {
                double y = c + i * hy;
                System.out.printf("│ %9.2f ", y);
                for (int j = 0; j < 8; j++) {
                    double x = a + j * hx;
                    double u = Math.pow(x, 2) + Math.pow(y, 4) * x;
                    System.out.printf("│ %10.2f ", (Double.isNaN(u) ? 0 : u));
                }
                System.out.println("│");
            }
            System.out.println("└───────────" + "┴────────────".repeat(8) + "┘");
        }
}