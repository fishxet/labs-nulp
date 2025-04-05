using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace NumericalMethods
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                Console.WriteLine("Введіть функцію f(x), наприклад: 1 - x*x - 0.5*Math.Exp(x) або 1 - x*x - 0.5*e^x=0");
                string functionInput = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(functionInput))
                {
                    Console.WriteLine("Помилка: Пустий ввід функції");
                    return;
                }

                // Нормалізація вводу
                functionInput = Regex.Replace(functionInput, @"\s*= *0\s*$", "");
                functionInput = Regex.Replace(functionInput, @"e\^(\w+)", "Math.Exp($1)");
                functionInput = Regex.Replace(functionInput, @"(\w+)\^(\w+)", "Math.Pow($1,$2)");

                Func<double, double> f = await CompileFunction(functionInput);

                Console.Write("Введіть початковий інтервал [a, b] (через пробіл): ");
                double[] interval = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
                double a = interval[0], b = interval[1];

                if (a > b)
                {
                    (a, b) = (b, a);
                    Console.WriteLine($"Зауваження: a > b, інтервал змінено на [{a}, {b}]");
                }

                Console.WriteLine("\nПеревірка умови Ліпшиця:");
                double? lipschitzConstant = EstimateLipschitzConstant(f, a, b);
                if (lipschitzConstant.HasValue)
                {
                    Console.WriteLine($"Функція задовольняє умові Ліпшиця з константою L ≈ {lipschitzConstant.Value:E2}");
                }
                else
                {
                    Console.WriteLine("Функція не задовольняє умові Ліпшиця (похідна необмежена або має розриви)");
                }

                Console.Write("\nВведіть початкове наближення x0: ");
                double x0 = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

                Console.Write("Введіть x1 для методу січних: ");
                double x1 = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

                Console.Write("Введіть список точностей (через пробіл, наприклад 1e-5 1e-6): ");
                double[] epsilons = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();

                // Автоматичне створення φ(x) = x - f(x)/K
                Func<double, double> phi = null;
                if (lipschitzConstant.HasValue)
                {
                 double K = lipschitzConstant.Value + 1;
                 const double h = 1e-8;
                 double sampleX = (a + b) / 2;
                 double derivativeSample = (f(sampleX + h) - f(sampleX)) / h;

                    if (derivativeSample < 0)
                    {
                    phi = x => x + f(x) / K;
                    Console.WriteLine($"\nАвтоматично згенерована ітераційна функція: φ(x) = x + f(x)/K, де K = {K:F2}");
                    }
                    else
                    {
                    phi = x => x - f(x) / K;
                    Console.WriteLine($"\nАвтоматично згенерована ітераційна функція: φ(x) = x - f(x)/K, де K = {K:F2}");
                    }
                }

                foreach (var epsilon in epsilons)
                {
                    Console.WriteLine($"\nРезультати для точності {epsilon:E1}:");

                    Console.WriteLine("\n[Метод бісекції]");
                    Bisection(f, a, b, epsilon);

                    Console.WriteLine("\n[Метод Ньютона]");
                    Newton(f, x0, epsilon);

                    Console.WriteLine("\n[Метод січних]");
                    Secant(f, x0, x1, epsilon);

                    if (phi != null)
                    {
                        Console.WriteLine("\n[Метод простої ітерації]");
                        SimpleIteration(phi, f, x0, epsilon);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }

        static double? EstimateLipschitzConstant(Func<double, double> f, double a, double b, int samples = 1000)
        {
            const double h = 1e-8;
            double maxDerivative = 0;
            bool isValid = true;

            for (int i = 0; i <= samples; i++)
            {
                double x = a + i * (b - a) / samples;
                try
                {
                    double derivative = (f(x + h) - f(x)) / h;
                    if (double.IsNaN(derivative) || double.IsInfinity(derivative))
                    {
                        isValid = false;
                        break;
                    }
                    maxDerivative = Math.Max(maxDerivative, Math.Abs(derivative));
                }
                catch
                {
                    isValid = false;
                    break;
                }
            }

            return isValid ? maxDerivative : (double?)null;
        }

        static async Task<Func<double, double>> CompileFunction(string expression)
        {
            string code = $"(double x) => {expression}";
            var options = ScriptOptions.Default
                .WithReferences(typeof(Math).Assembly)
                .WithImports("System");

            return await CSharpScript.EvaluateAsync<Func<double, double>>(code, options);
        }

        static void Bisection(Func<double, double> f, double a, double b, double epsilon)
        {
            if (f(a) * f(b) >= 0)
            {
                Console.WriteLine("Неможливо застосувати метод: f(a) та f(b) мають однаковий знак");
                return;
            }

            int iter = 0;
            double aCurrent = a, bCurrent = b;
            while (bCurrent - aCurrent > epsilon)
            {
                double c = (aCurrent + bCurrent) / 2;
                if (f(aCurrent) * f(c) < 0)
                    bCurrent = c;
                else
                    aCurrent = c;
                iter++;
            }
            double root = (aCurrent + bCurrent) / 2;
            Console.WriteLine($"Корінь: {root:F10}, Ітерації: {iter}, Нев'язка: {Math.Abs(f(root)):E2}");
        }

        static void Newton(Func<double, double> f, double x0, double epsilon)
        {
            Func<double, double> dfdx = x => (f(x + 1e-8) - f(x)) / 1e-8;

            int iter = 0;
            double x = x0, delta;
            do
            {
                double fx = f(x);
                double dfx = dfdx(x);
                delta = fx / dfx;
                x -= delta;
                iter++;
            } while (Math.Abs(delta) > epsilon && iter < 1000);

            Console.WriteLine($"Корінь: {x:F10}, Ітерації: {iter}, Нев'язка: {Math.Abs(f(x)):E2}");
        }

        static void Secant(Func<double, double> f, double x0, double x1, double epsilon)
        {
            int iter = 0;
            double xPrev = x0, xCurrent = x1, xNext;
            do
            {
                double fPrev = f(xPrev);
                double fCurrent = f(xCurrent);
                xNext = xCurrent - fCurrent * (xCurrent - xPrev) / (fCurrent - fPrev);
                xPrev = xCurrent;
                xCurrent = xNext;
                iter++;
            } while (Math.Abs(xCurrent - xPrev) > epsilon && iter < 1000);

            Console.WriteLine($"Корінь: {xCurrent:F10}, Ітерації: {iter}, Нев'язка: {Math.Abs(f(xCurrent)):E2}");
        }

     static void SimpleIteration(Func<double, double> phi, Func<double, double> f, double x0, double epsilon)
{
    int iter = 0;
    double x = x0, prevX;
    double residual = double.MaxValue;
    do
    {
        prevX = x;
        x = phi(prevX);
        residual = Math.Abs(f(x));
        iter++;
        if (iter >= 1000 || double.IsNaN(x))
            break;
    } while (Math.Abs(x - prevX) > epsilon || residual > epsilon);

    if (double.IsNaN(x))
        Console.WriteLine("Розв'язок не знайдено (NaN)");
    else
        Console.WriteLine($"Корінь: {x:F10}, Ітерації: {iter}, Нев'язка: {residual:E2}");
}
    }
}