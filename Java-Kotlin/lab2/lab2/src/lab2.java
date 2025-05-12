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
        scanner.useLocale(Locale.US);
        double a, b, c;

        try {
            System.out.print("Введіть a, b, c (через пробіл): ");
            a = scanner.nextDouble();
            b = scanner.nextDouble();
            c = scanner.nextDouble();
        } catch (InputMismatchException e) {
            System.out.println("Помилка: некоректний формат чисел.");
            return;
        } finally {
            scanner.close();
        }

        double D = b*b - 4*c;
        // випадок D<0: знаменник >0 завжди ⇒ x<a
        if (D < 0) {
            System.out.printf("Розв’язок: (-∞; %.2f)%n", a);
            return;
        }

        // знаходимо корені знаменника
        double sqrtD = Math.sqrt(D);
        double x1 = (-b - sqrtD) / 2;
        double x2 = (-b + sqrtD) / 2;
        if (x1 > x2) { double t = x1; x1 = x2; x2 = t; }

        StringBuilder sol = new StringBuilder();
        boolean first = true;

        // 1) (-∞, x1): знаменник >0 ⇒ x<a ⇒ x<min(a,x1)
        double end1 = Math.min(a, x1);
        // якщо хочемо тільки непорожні інтервали:
        if (end1 > Double.NEGATIVE_INFINITY) {
            sol.append(String.format("(-∞; %.2f)", end1));
            first = false;
        }

        // 2) (x1, x2): знаменник <0 ⇒ x>a ⇒ x>max(a,x1)
        double start2 = Math.max(a, x1);
        if (start2 < x2) {
            if (!first) sol.append(" ∪ ");
            sol.append(String.format("(%.2f; %.2f)", start2, x2));
            first = false;
        }

        // 3) (x2, +∞): знаменник >0 ⇒ x<a ⇒ x< a ⇒ (x2, a)
        if (a > x2) {
            if (!first) sol.append(" ∪ ");
            sol.append(String.format("(%.2f; %.2f)", x2, a));
        }

        System.out.println("Розв’язок: " + sol.toString());
    }
    public static void main(String[] args) {
        LessNumber();
        InequalitySolver();
    }
}
