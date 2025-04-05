import java.util.*

object lab1kot {
    fun FahrenheitToCelsius() {
        val scanner = Scanner(System.`in`)
        var tf = 0.0
        var isValid = false

        while (!isValid) {
            try {
                print("Введіть температуру у градусах Фаренгейта: ")
                val input = scanner.nextLine().replace(',', '.') // Обробка коми як десяткового роздільника
                tf = input.toDouble()
                isValid = true
            } catch (e: NumberFormatException) {
                println("Помилка: введіть числове значення (наприклад, 98.6 або -40).")
            }
        }

        val tc = (tf - 32) * 5 / 9
        System.out.printf("Температура в градусах Цельсія: %.2f\n", tc)
    }

    fun HundreedsDigit() {
        val scanner = Scanner(System.`in`)
        var n = 0
        var isValid = false

        while (!isValid) {
            try {
                print("Введіть число більше 999: ")
                val input = scanner.nextLine()
                n = input.toInt()
                if (n > 999) {
                    isValid = true
                } else {
                    println("Помилка: число має бути більше 999.")
                }
            } catch (e: NumberFormatException) {
                println("Помилка: введіть ціле число (наприклад, 1234).")
            }
        }

        val hundreds = (n / 100) % 10
        println("Цифра сотень: $hundreds")
    }

    @JvmStatic
    fun main(args: Array<String>) {
        FahrenheitToCelsius()
        HundreedsDigit()
    }
}