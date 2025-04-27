using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace InterpolationLab
{

    public class Node
    {
        [XmlAttribute]
        public double X { get; set; }
        [XmlAttribute]
        public double Y { get; set; }
    }
    public class FunctionData
    {
        public string Name { get; set; }
        [XmlArray("Nodes")]
        [XmlArrayItem("Node")]
        public List<Node> Nodes { get; set; }
    }
    public class InterpolationJob
    {
        public FunctionData Function { get; set; }
        [XmlArray("Points")]
        [XmlArrayItem("Point")]
        public List<double> Points { get; set; }
    }

    // Кореневий елемент для читання пакетів завдань
    [XmlRoot("InterpolationJobs")]
    public class InputBatch
    {
        [XmlElement("InterpolationJob")]
        public List<InterpolationJob> Jobs { get; set; }
    }

    // Калькулятор розділених різниць
    public class DividedDifferenceCalculator
    {
        private readonly double[,] table;
        public double[] Coefficients { get; }
        public double[] Nodes { get; }

        public DividedDifferenceCalculator(double[] x, double[] y)
        {
            int n = x.Length;
            Nodes = x;
            table = new double[n, n];
            Coefficients = new double[n];
            for (int i = 0; i < n; i++) table[i, 0] = y[i];
            for (int j = 1; j < n; j++)
                for (int i = 0; i < n - j; i++)
                    table[i, j] = (table[i + 1, j - 1] - table[i, j - 1]) / (x[i + j] - x[i]);
            for (int i = 0; i < n; i++) Coefficients[i] = table[0, i];
        }
    }

    // Інтерполятор за схемою Горнера
    public class NewtonInterpolator
    {
        private readonly double[] nodes;
        private readonly double[] coeffs;

        public NewtonInterpolator(double[] nodes, double[] coeffs)
        {
            this.nodes = nodes;
            this.coeffs = coeffs;
        }

        public double Evaluate(double t)
        {
            int n = coeffs.Length;
            double result = coeffs[n - 1];
            for (int i = n - 2; i >= 0; i--)
                result = result * (t - nodes[i]) + coeffs[i];
            return result;
        }
    }

    // Головна програма: читає input.xml з багатьма завданнями, обчислює, пише output.txt
    public static class Program
    {
        public static void Main()
        {
            const string inputPath = "input.xml";
            const string outputPath = "output.txt";

            // Десеріалізація пакету завдань з XML
            InputBatch batch;
            var serializerIn = new XmlSerializer(typeof(InputBatch));
            using (var fs = new FileStream(inputPath, FileMode.Open))
                batch = (InputBatch)serializerIn.Deserialize(fs);

            // Створюємо файл виводу
            using (var writer = new StreamWriter(outputPath))
            {
                // Обробка кожного завдання
                foreach (var job in batch.Jobs)
                {
                    // Заголовок функції
                    writer.WriteLine($"---- {job.Function.Name} ----");

                    // Підготовка вузлів
                    int n = job.Function.Nodes.Count;
                    double[] xs = new double[n], ys = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        xs[i] = job.Function.Nodes[i].X;
                        ys[i] = job.Function.Nodes[i].Y;
                    }

                    // Розрахунок розділених різниць та інтерполятор
                    var dd = new DividedDifferenceCalculator(xs, ys);
                    var interp = new NewtonInterpolator(dd.Nodes, dd.Coefficients);

                    // Обчислення і вивід результатів з округленням до 4 знаків
                    foreach (var x in job.Points)
                    {
                        double y = interp.Evaluate(x);
                        double xRound = Math.Round(x, 4);
                        double yRound = Math.Round(y, 4);
                        writer.WriteLine($"f({xRound:F4}) = {yRound:F4}");
                    }
                    writer.WriteLine(); // порожній рядок між функціями
                }
            }
            Console.WriteLine($"Результати записані у {outputPath}");
        }
    }
}