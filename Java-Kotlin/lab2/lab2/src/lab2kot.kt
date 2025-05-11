import java.util.*
import kotlin.math.max
import kotlin.math.min
import kotlin.math.sqrt

object lab2kot {
    fun LessNumber() {
        val scanner = Scanner(System.`in`)
        val nums = IntArray(3)
        val labels = arrayOf("перше", "друге", "третє")

        var i = 0
        while (i < 3) {
            try {
                System.out.printf("Введіть %s ціле число: ", labels[i])
                nums[i] = scanner.nextLine().toInt()
                i++
            } catch (e: NumberFormatException) {
                println("Помилка: введіть ціле число.")
            }
        }

        println("Найменше число: " + min(nums[0].toDouble(), min(nums[1].toDouble(), nums[2].toDouble())))
    }

    fun InequalitySolver() {
        val scanner = Scanner(System.`in`)
        scanner.useLocale(Locale.US)
        var a: Double
        var b: Double
        var c: Double

        try {
            print("Введіть a, b, c (через пробіл): ")
            a = scanner.nextDouble()
            b = scanner.nextDouble()
            c = scanner.nextDouble()
        } catch (e: InputMismatchException) {
            println("Помилка: некоректний формат чисел.")
            return
        } finally {
            scanner.close()
        }

        val D = b * b - 4 * c
        // випадок D<0: знаменник >0 завжди ⇒ x<a
        if (D < 0) {
            System.out.printf("Розв’язок: (-∞; %.2f)%n", a)
            return
        }

        // знаходимо корені знаменника
        val sqrtD = sqrt(D)
        var x1 = (-b - sqrtD) / 2
        var x2 = (-b + sqrtD) / 2
        if (x1 > x2) {
            val t = x1
            x1 = x2
            x2 = t
        }

        val sol = StringBuilder()
        var first = true

        // 1) (-∞, x1): знаменник >0 ⇒ x<a ⇒ x<min(a,x1)
        val end1 = min(a, x1)
        // якщо хочемо тільки непорожні інтервали:
        if (end1 > Double.NEGATIVE_INFINITY) {
            sol.append(String.format("(-∞; %.2f)", end1))
            first = false
        }

        // 2) (x1, x2): знаменник <0 ⇒ x>a ⇒ x>max(a,x1)
        val start2 = max(a, x1)
        if (start2 < x2) {
            if (!first) sol.append(" ∪ ")
            sol.append(String.format("(%.2f; %.2f)", start2, x2))
            first = false
        }

        // 3) (x2, +∞): знаменник >0 ⇒ x<a ⇒ x< a ⇒ (x2, a)
        if (a > x2) {
            if (!first) sol.append(" ∪ ")
            sol.append(String.format("(%.2f; %.2f)", x2, a))
        }

        println("Розв’язок: $sol")
    }

    @JvmStatic
    fun main(args: Array<String>) {
        LessNumber()
        InequalitySolver()
    }
}