using System;
using System.IO;
using System.Linq;
using Accord.MachineLearning;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

namespace Claster
{
    class Program
    {
        static void Main(string[] args)
        {
            // Генерація даних
            double[][] data = GenerateTestData(19999);
            
            // Створення графіка кластерів
            CreateClusterPlot(data, 7, "KMeansClustering.png");
            
            // Створення графіка методу ліктя
            CreateElbowMethodPlot(data, 3, "ElbowMethod.png");
        }

        static void CreateClusterPlot(double[][] data, int k, string fileName)
        {
            var kmeans = new KMeans(k);
            var clusters = kmeans.Learn(data);
            int[] labels = clusters.Decide(data);

            var plotModel = new PlotModel { Title = $"Кластеризація K-Means (k={k})" };
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            OxyColor[] colors = { OxyColors.Blue, OxyColors.AntiqueWhite, OxyColors.Red, OxyColors.Orange,OxyColors.Orange, OxyColors.Gray };
            
            for (int cluster = 0; cluster < k; cluster++)
            {  
                var series = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2,
                    MarkerFill = colors[(cluster + k) % colors.Length],
                };

                foreach (var point in data.Where((p, i) => labels[i] == cluster))
                {
                    series.Points.Add(new ScatterPoint(point[0], point[1]));
                }
                
                plotModel.Series.Add(series);
            }

            SavePlot(plotModel, fileName);
        }

        static void CreateElbowMethodPlot(double[][] data, int maxK, string fileName)
        {
            var errors = new double[maxK];
            for (int k = 1; k <= maxK; k++)
            {
                var kmeans = new KMeans(k);
                var clusters = kmeans.Learn(data);
                errors[k-1] = kmeans.Error; // Сума квадратів відстаней
            }

            var plotModel = new PlotModel { Title = "Метод Ліктя (Визначення оптимального K)" };
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Кількість кластерів (k)" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Сума квадратів відстаней" });

            var series = new LineSeries
            {
                Color = OxyColors.Blue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColors.Red
            };

            for (int i = 0; i < maxK; i++)
            {
                series.Points.Add(new DataPoint(i+1, errors[i]));
            }

            plotModel.Series.Add(series);
            SavePlot(plotModel, fileName);
        }

        static void SavePlot(PlotModel plotModel, string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            Console.WriteLine($"Зберігаємо файл: {filePath}");

            try
            {
                using (var stream = File.Create(filePath))
                {
                    var exporter = new PngExporter { Width = 600, Height = 400 };
                    exporter.Export(plotModel, stream);
                }
                Console.WriteLine($"Файл {fileName} успішно створено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }

        static double[][] GenerateTestData(int totalPoints)
        {
            // Генерація даних (без змін)
            var rand = new Random();
            double[][] centers = {
                new[]{ 0.1, 0.2 },
                new[]{ 0.2, 0.3 },
                new[]{ 0.4, 0.5 },
                new[]{ 0.4, 0.6 },
                new[]{ 0.1, 0.7 },
                new[]{ 0.5, 0.2 },
                new[]{ 0.7, 0.8 },
                new[]{ 0.8, 0.9}
            };

            return Enumerable.Range(0, totalPoints)
                .Select(i => {
                    var center = centers[rand.Next(centers.Length)];
                    return new[] { 
                        center[0] + 1 * (rand.NextDouble() - 0.2),
                        center[1] + 1 * (rand.NextDouble() - 0.2)
                    };
                }).ToArray();
        }
    }
}