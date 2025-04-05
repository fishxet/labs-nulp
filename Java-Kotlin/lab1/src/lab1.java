import java.util.Scanner;

public class lab1 {
    public static void FahrenheitToCelsius() {
        Scanner scanner = new Scanner(System.in);
        double tf = 0;
        boolean isValid = false;

        while (!isValid) {
            try {
                System.out.print("Введіть температуру у градусах Фаренгейта: ");
                String input = scanner.nextLine().replace(',', '.'); // Обробка коми як десяткового роздільника
                tf = Double.parseDouble(input);
                isValid = true;
            } catch (NumberFormatException e) {
                System.out.println("Помилка: введіть числове значення (наприклад, 98.6 або -40).");
            }
        }

        double tc = (tf - 32) * 5 / 9;
        System.out.printf("Температура в градусах Цельсія: %.2f\n", tc);
    }
    public static void HundreedsDigit() {
        Scanner scanner = new Scanner(System.in);
        int n = 0;
        boolean isValid = false;

        while (!isValid) {
            try {
                System.out.print("Введіть число більше 999: ");
                String input = scanner.nextLine();
                n = Integer.parseInt(input);
                if (n > 999) {
                    isValid = true;
                } else {
                    System.out.println("Помилка: число має бути більше 999.");
                }
            } catch (NumberFormatException e) {
                System.out.println("Помилка: введіть ціле число (наприклад, 1234).");
            }
        }

        int hundreds = (n / 100) % 10;
        System.out.println("Цифра сотень: " + hundreds);

    }
    public static void main(String[] args) {
        FahrenheitToCelsius();
        HundreedsDigit();
    }
}
