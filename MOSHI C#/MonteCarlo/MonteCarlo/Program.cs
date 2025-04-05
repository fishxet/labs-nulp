using System;
using ScottPlot;

class MonteCarloIntegration
{
    static void Main()
    {
        // Тестова функція: f(x) = x² на [0, 1]
        double TestFunc(double x) => Math.Pow(x, 2);
        double ExactTestIntegral() => 1.0 / 3;

        // Основна функція: f(x) = e^(x²) на [1, 2]
        double MainFunc(double x) => Math.Exp(Math.Pow(x, 2));

        // Параметри
        int N = 10000;
        double a_test = 0, b_test = 1;
        double a_main = 1, b_main = 2;
        double yMax_test = 1;
        double yMax_main = Math.Exp(4);

        // Обчислення інтегралів
        (double testResult, var testPlot) = MonteCarlo(TestFunc, a_test, b_test, yMax_test, N);
        (double mainResult, var mainPlot) = MonteCarlo(MainFunc, a_main, b_main, yMax_main, N);

        // Вивід результатів
        Console.WriteLine("Тестовий інтеграл (x² [0,1]):");
        Console.WriteLine($"Точне значення: {ExactTestIntegral():F5}");
        Console.WriteLine($"Монте-Карло: {testResult:F5}");
        Console.WriteLine($"Абс. похибка: {Math.Abs(testResult - ExactTestIntegral()):F5}\n");

        Console.WriteLine($"Основний інтеграл (e^(x²) [1,2]): {mainResult:F5}");

        // Збереження графіків
        testPlot.SavePng("test_plot.png", 1920, 1080);
        mainPlot.SavePng("main_plot.png", 1920, 1080);
    }

    static (double result, Plot plt) MonteCarlo(Func<double, double> func, double a, double b, double yMax, int n)
    {
        Random rand = new();
        Plot plt = new();
        int pointsUnderCurve = 0;

        // Генерація точок
        double[] xUnder = new double[n];
        double[] yUnder = new double[n];
        double[] xOver = new double[n];
        double[] yOver = new double[n];

        for (int i = 0; i < n; i++)
        {
            double x = a + (b - a) * rand.NextDouble();
            double y = yMax * rand.NextDouble();
            
            if (y <= func(x))
            {
                xUnder[pointsUnderCurve] = x;
                yUnder[pointsUnderCurve] = y;
                pointsUnderCurve++;
            }
            else
            {
                xOver[i - pointsUnderCurve] = x;
                yOver[i - pointsUnderCurve] = y;
            }
        }

        // Додавання точок на графік двома окремими шарами
        var underScatter = plt.Add.Scatter(xUnder, yUnder);
        underScatter.Color = ScottPlot.Color.FromHex("#1f77b4"); // Синій
        underScatter.MarkerSize = 3;

        var overScatter = plt.Add.Scatter(xOver, yOver);
        overScatter.Color = ScottPlot.Color.FromHex("#ff7f0e"); // Помаранчевий
        overScatter.MarkerSize = 3;

        plt.Title($"Метод Монте-Карло (N = {n:n0})");
        plt.XLabel("x");
        plt.YLabel("y");

        // Розрахунок інтеграла
        double area = (b - a) * yMax;
        double integral = area * pointsUnderCurve / n;

        return (integral, plt);
    }
}