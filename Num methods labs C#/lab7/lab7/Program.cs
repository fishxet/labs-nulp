using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace Lab7Quadrature
{
    public class IntegrationJob
    {
        // Тип формули: тут фіксуємо 3 для Сімпсона
        public int FormulaType { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double Epsilon { get; set; }
    }

    [XmlRoot("IntegrationJob")]
    public class InputData : IntegrationJob { }

    public static class Program
    {
        // Підінтегральна функція f(x) = sqrt(1 + x^5)
        public static double F(double x) => Math.Sqrt(1 + Math.Pow(x, 5));

        // Складена формула Сімпсона
        public static double CompositeSimpson(double a, double b, int n)
        {
            if (n % 2 != 0) n++;
            double h = (b - a) / n;
            double sum = F(a) + F(b);
            for (int i = 1; i < n; i++)
                sum += F(a + h * i) * (i % 2 == 0 ? 2 : 4);
            return sum * h / 3;
        }

        public static void Main()
        {
            const string inputPath = "input_lab7.xml";
            const string outputPath = "output_lab7.txt";

            // Зчитування вхідних даних
            InputData job;
            var serializer = new XmlSerializer(typeof(InputData));
            using (var fs = new FileStream(inputPath, FileMode.Open))
                job = (InputData)serializer.Deserialize(fs);

            double a = job.A, b = job.B, eps = job.Epsilon;
            int n = 8;           // початкове число підінтервалів для Simpson: має бути парним
            int formula = job.FormulaType;
            int p = 4;           // порядок Сімпсона

            // Обираємо формулу: сюди лише Сімпсон
            Func<double, double, int, double> quad = CompositeSimpson;

            using (var writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("Лабораторна робота №7. Варіант 18");
                writer.WriteLine("Складена формула Сімпсона");
                writer.WriteLine($"Інтеграл від sqrt(1 + x^5) на [ {a}, {b} ], точність eps = {eps}");
                writer.WriteLine();

                double I1, I2, error;
                while (true)
                {
                    I1 = quad(a, b, n);
                    I2 = quad(a, b, n * 2);
                    error = Math.Abs(I2 - I1) / (Math.Pow(2, p) - 1);
                    writer.WriteLine($"N = {n}, I(h) = {I1:F8}, I(h/2) = {I2:F8}, оцінка похибки за Рунге = {error:E8}");
                    if (error < eps) break;
                    n *= 2;
                }

                // Екстраполяція Рунге
                double I_extrap = I2 + (I2 - I1) / (Math.Pow(2, p) - 1);
                writer.WriteLine();
                writer.WriteLine($"Уточнене значення інтегралу (екстраполяція): {I_extrap:F8}");
            }

            Console.WriteLine($"Результати записані у {outputPath}");
        }
    }
}
