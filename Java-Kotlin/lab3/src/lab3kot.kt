import java.util.*
import kotlin.math.abs
import kotlin.math.atan
import kotlin.math.pow

object lab3kot {
    @JvmStatic
    fun main(args: Array<String>) {
        task1()
        task2()
    }

    // Завдання 1: Обчислення arccotg(x)
    fun task1() {
        val scanner = Scanner(System.`in`)
        println("\n=== Завдання 1: Обчислення arccotg(x) ===")
        var x = 0.0

        // Обробка введення з перевірками
        while (true) {
            try {
                print("Введіть x (x ≠ 0): ")
                x = scanner.nextDouble()
                require(x != 0.0)
                break
            } catch (e: InputMismatchException) {
                println("Помилка: Очікується число!")
                scanner.nextLine()
            } catch (e: IllegalArgumentException) {
                println("Помилка: x не може бути нулем!")
            }
        }

        // Обчислення ряду
        val epsilon = 0.00001
        var sum = Math.PI / 2
        var term = 1.0 / (2 * 0 + 1)
        var k = 0

        while (abs(term) >= epsilon) {
            sum -= (-1.0).pow(k.toDouble()) * term
            k++
            term = x.pow((2 * k).toDouble()) / (2 * k + 1)
        }

        // Вивід результатів
        println("\n┌───────────────────────────────┐")
        System.out.printf("│%-15s %12.6f │\n", "Ряд:", sum)
        System.out.printf("│%-15s %12.6f │\n", "Бібліотека:", atan(1 / x))
        System.out.printf("│%-15s %12d │\n", "Доданків:", k)
        println("└───────────────────────────────┘")
    }

    // Завдання 2: Табулювання функції u = x² + y⁴x
    fun task2() {
        val scanner = Scanner(System.`in`)
        println("\n=== Завдання 2: Табулювання функції u = x² + y⁴x ===")

        // Обробка введення меж
        var a = 0.0
        var b = 0.0
        var c = 0.0
        var d = 0.0
        while (true) {
            try {
                print("Введіть межі x [a b]: ")
                a = scanner.nextDouble()
                b = scanner.nextDouble()
                print("Введіть межі y [c d]: ")
                c = scanner.nextDouble()
                d = scanner.nextDouble()
                if (a > b) {
                    val tmp = a
                    a = b
                    b = tmp
                }
                if (c > d) {
                    val tmp = c
                    c = d
                    d = tmp
                }
                break
            } catch (e: InputMismatchException) {
                println("Помилка: Введіть числа!")
                scanner.nextLine()
            }
        }

        // Обчислення кроків
        val hx = (b - a) / 7
        val hy = (d - c) / 7

        // Вивід таблиці з форматуванням
        println(
            """
                
                ┌───────────${"┬────────────".repeat(8)}┐
                """.trimIndent()
        )
        System.out.printf("│ %-9s ", "y\\x")
        for (i in 0..7) {
            System.out.printf("│ %10.2f ", a + i * hx)
        }
        println(
            """
                │
                ├───────────${"┼────────────".repeat(8)}┤
                """.trimIndent()
        )

        // Тіло таблиці
        for (i in 0..7) {
            val y = c + i * hy
            System.out.printf("│ %9.2f ", y)
            for (j in 0..7) {
                val x = a + j * hx
                val u = x.pow(2.0) + y.pow(4.0) * x
                System.out.printf("│ %10.2f ", (if (java.lang.Double.isNaN(u)) 0.0 else u))
            }
            println("│")
        }
        println("└───────────" + "┴────────────".repeat(8) + "┘")
    }
}