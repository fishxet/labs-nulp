// Лабораторна робота №6: Приближення функцій методом найменших квадратів
// Читання даних з input_lab6.xml та запис результатів у output.txt
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Lab6LeastSquares
{
    // Опис вузла табличних даних
    public class Node
    {
        [XmlAttribute]
        public double X { get; set; }
        [XmlAttribute]
        public double Y { get; set; }
    }

    // Дані функції для одного завдання
    public class FunctionData
    {
        public string Name { get; set; }
        [XmlArray("Nodes")]
        [XmlArrayItem("Node")]
        public List<Node> Nodes { get; set; }
    }

    // Модель одного завдання МНК
    public class LSJob
    {
        public FunctionData Function { get; set; }
    }

    // Кореневий елемент для читання XML-пакету завдань
    [XmlRoot("LSJobs")]
    public class InputBatch
    {
        [XmlElement("LSJob")]
        public List<LSJob> Jobs { get; set; }
    }

    // Головний клас програми
    public static class Program
    {
        public static void Main()
        {
            const string inputPath = "input_lab6.xml";
            const string outputPath = "output.txt";

            // Десеріалізація вхідного файлу
            InputBatch batch;
            var serializerIn = new XmlSerializer(typeof(InputBatch));
            using (var fs = new FileStream(inputPath, FileMode.Open))
                batch = (InputBatch)serializerIn.Deserialize(fs);

            // Відкриваємо файл для запису результатів
            using (var writer = new StreamWriter(outputPath))
            {
                foreach (var job in batch.Jobs)
                {
                    var name = job.Function.Name;
                    writer.WriteLine($"---- {name} ----");

                    // Визначення очікуваних вузлів: x від 0.0 до 0.9 з кроком 0.1
                    double[] expectedX = Enumerable.Range(0, 10).Select(i => Math.Round(i * 0.1, 1)).ToArray();

                    // Існуючі вузли даних
                    var nodes = job.Function.Nodes;
                    var xs = nodes.Select(n => n.X).ToArray();
                    var ys = nodes.Select(n => n.Y).ToArray();
                    int n = xs.Length;

                    // Визначення та вивід пропущених точок
                    var missingX = expectedX.Except(xs).ToArray();
                    if (missingX.Any())
                    {
                        writer.WriteLine("Пропущені значення аргументу x (будуть розраховані):");
                        writer.WriteLine(string.Join(", ", missingX.Select(x => x.ToString("F1"))));
                        writer.WriteLine();
                    }

                    // Поліном 0-го степеня: y = a0 (постійна)
                    double a0Const = ys.Average();
                    var deviations0 = ys.Select(y => Math.Abs(y - a0Const)).ToArray();
                    double sumOfSquaredErrors0 = deviations0.Select(e => e * e).Sum();
                    double maxDeviation0 = deviations0.Max();
                    double meanSquaredError0 = sumOfSquaredErrors0 / n;
                    double rootMeanSquaredError0 = Math.Sqrt(meanSquaredError0);

                    writer.WriteLine("Наближення 0-го степеня (постійний поліном)");
                    writer.WriteLine($"  Постійний коефіцієнт a0 = {Math.Round(a0Const, 4):F4}");
                    writer.WriteLine($"  Сума квадратів похибок (SSE) = {Math.Round(sumOfSquaredErrors0, 4):F4}");
                    writer.WriteLine($"  Середньоквадратична помилка (MSE) = {Math.Round(meanSquaredError0, 4):F4}");
                    writer.WriteLine($"  Корінь середньоквадратичної помилки (RMSE) = {Math.Round(rootMeanSquaredError0, 4):F4}");
                    writer.WriteLine($"  Максимальна абсолютна похибка = {Math.Round(maxDeviation0, 4):F4}");
                    writer.WriteLine();

                    // Поліном 1-го степеня: y = a1*x + a0
                    double sumX = xs.Sum();
                    double sumY = ys.Sum();
                    double sumXY = xs.Zip(ys, (x, y) => x * y).Sum();
                    double sumX2 = xs.Select(x => x * x).Sum();
                    double denominator = n * sumX2 - sumX * sumX;
                    double a1 = (n * sumXY - sumX * sumY) / denominator;
                    double a0Linear = (sumY - a1 * sumX) / n;
                    var deviations1 = xs.Zip(ys, (x, y) => Math.Abs(y - (a1 * x + a0Linear))).ToArray();
                    double sumOfSquaredErrors1 = deviations1.Select(e => e * e).Sum();
                    double maxDeviation1 = deviations1.Max();
                    double meanSquaredError1 = sumOfSquaredErrors1 / n;
                    double rootMeanSquaredError1 = Math.Sqrt(meanSquaredError1);

                    writer.WriteLine("Наближення 1-го степеня (лінійний поліном)");
                    writer.WriteLine($"  Коефіцієнт нахилу a1 = {Math.Round(a1, 4):F4}");
                    writer.WriteLine($"  Вільний член a0 = {Math.Round(a0Linear, 4):F4}");
                    writer.WriteLine($"  Сума квадратів похибок (SSE) = {Math.Round(sumOfSquaredErrors1, 4):F4}");
                    writer.WriteLine($"  Середньоквадратична помилка (MSE) = {Math.Round(meanSquaredError1, 4):F4}");
                    writer.WriteLine($"  Корінь середньоквадратичної помилки (RMSE) = {Math.Round(rootMeanSquaredError1, 4):F4}");
                    writer.WriteLine($"  Максимальна абсолютна похибка = {Math.Round(maxDeviation1, 4):F4}");
                    writer.WriteLine();

                    // Розрахунок значень у пропущених точках за лінійним поліномом
                    if (missingX.Any())
                    {
                        writer.WriteLine("Розраховані значення y для пропущених x за допомогою лінійного полінома:");
                        foreach (var xm in missingX)
                        {
                            double ym = a1 * xm + a0Linear;
                            writer.WriteLine($"  x = {xm:F1}  ->  y ≈ {Math.Round(ym, 4):F4}");
                        }
                        writer.WriteLine();
                    }
                }
            }

            Console.WriteLine($"Результати записані у {outputPath}");
        }
    }
}
