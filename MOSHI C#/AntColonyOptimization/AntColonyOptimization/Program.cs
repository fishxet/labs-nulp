using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Accord.MachineLearning;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

namespace ACO_TSP_Kaleidoscope
{
    class Program
    {
        static void Main(string[] args)
        {
            // Параметри задачі та мурашиного алгоритму
            int cityCount = 30;         // кількість міст
            int antCount = 20;          // кількість мурах
            int maxIterations = 100;    // кількість ітерацій

            double alpha = 1.0;         // вплив феромону
            double beta = 5.0;          // вплив евристики (1/відстань)
            double evaporation = 0.5;   // коефіцієнт випаровування феромону
            double Q = 500;             // константа для оновлення феромону

            // Генеруємо випадкові координати міст (хоча далі ми відобразимо їх у колі)
            var cities = GenerateCities(cityCount, 0, 1000);

            // Запускаємо мурашиний алгоритм
            var aco = new AntColonyOptimization(cities, antCount, alpha, beta, evaporation, Q);
            aco.Run(maxIterations);

            // Отримуємо результати
            var bestRoute = aco.BestRoute;        // послідовність індексів найкращого маршруту
            double bestDistance = aco.BestLength;   // довжина найкращого маршруту

            Console.WriteLine("Найкращий знайдений маршрут (індекси міст):");
            Console.WriteLine(string.Join(" ", bestRoute));
            Console.WriteLine($"Довжина маршруту: {bestDistance:F2}");

            // Генеруємо графічний вивід у стилі "каляйдоскоп"
            PlotKaleidoscopeRoutes(cityCount, bestRoute, "KaleidoscopeRoutes.png");

            Console.WriteLine("Готово! Файл KaleidoscopeRoutes.png створено.");
        }

        /// <summary>
        /// Генерує випадкові координати міст у заданому діапазоні.
        /// </summary>
        static (double x, double y)[] GenerateCities(int count, int minCoord, int maxCoord)
        {
            Random rand = new Random();
            var cities = new (double x, double y)[count];
            for (int i = 0; i < count; i++)
            {
                double x = rand.NextDouble() * (maxCoord - minCoord) + minCoord;
                double y = rand.NextDouble() * (maxCoord - minCoord) + minCoord;
                cities[i] = (x, y);
            }
            return cities;
        }

        /// <summary>
        /// Створює графік-каляйдоскоп на основі оптимального маршруту.
        /// Міста розташовуються рівномірно по колу, а маршрут повторюється кілька разів з різними обертами.
        /// </summary>
        static void PlotKaleidoscopeRoutes(int cityCount, int[] route, string fileName)
        {
            // Задаємо радіус кола
            double R = 100;
            // Створюємо координати міст на колі: рівномірно розташовані по колу
            var circleCities = new (double x, double y)[cityCount];
            for (int i = 0; i < cityCount; i++)
            {
                double angle = 2 * Math.PI * i / cityCount;
                circleCities[i] = (R * Math.Cos(angle), R * Math.Sin(angle));
            }

            // Формуємо маршрут за оптимальним порядком міст
            List<DataPoint> bestRoutePoints = new List<DataPoint>();
            foreach (var idx in route)
            {
                bestRoutePoints.Add(new DataPoint(circleCities[idx].x, circleCities[idx].y));
            }
            // Закриваємо маршрут, додаючи першу точку
            bestRoutePoints.Add(bestRoutePoints[0]);

            // Створюємо модель графіка
            var plotModel = new PlotModel { Title = "Каляйдоскоп маршрутів" };
            // Встановлюємо межі осей для гарного відображення
            double bound = R * 1.3;
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = -bound, Maximum = bound, Title = "X" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = -bound, Maximum = bound, Title = "Y" });

            // Створюємо кілька копій маршруту, обернених навколо центра для ефекту "каляйдоскопу"
            int copies = 8;
            for (int i = 0; i < copies; i++)
            {
                double rotation = 2 * Math.PI * i / copies;
                var series = new LineSeries
                {
                    Color = OxyColor.FromAColor(150, OxyColors.Blue),
                    StrokeThickness = 1.5,
                    Title = $"Оберт {i + 1}"
                };

                foreach (var point in bestRoutePoints)
                {
                    double rotatedX = point.X * Math.Cos(rotation) - point.Y * Math.Sin(rotation);
                    double rotatedY = point.X * Math.Sin(rotation) + point.Y * Math.Cos(rotation);
                    series.Points.Add(new DataPoint(rotatedX, rotatedY));
                }
                plotModel.Series.Add(series);
            }

            // Додаємо точки міст (червоні кружечки)
            var scatter = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColors.Red,
                Title = "Міста"
            };

            foreach (var pt in circleCities)
            {
                scatter.Points.Add(new ScatterPoint(pt.x, pt.y));
            }
            plotModel.Series.Add(scatter);

            // Експортуємо графік у PNG
            using (var stream = File.Create(fileName))
            {
                PngExporter.Export(plotModel, stream, 800, 800);
            }
        }
    }

    /// <summary>
    /// Клас, що реалізує мурашиний алгоритм для задачі комівояжера.
    /// </summary>
    class AntColonyOptimization
    {
        private (double x, double y)[] _cities; // координати міст
        private int _cityCount;                 // кількість міст
        private int _antCount;                  // кількість мурах

        private double[,] _distances;           // матриця відстаней
        private double[,] _pheromones;          // матриця феромонів
        private double[,] _eta;                 // евристична матриця (1/відстань)

        private double _alpha;                  // вплив феромону
        private double _beta;                   // вплив евристики
        private double _evaporation;            // коефіцієнт випаровування
        private double _Q;                      // константа для оновлення феромону

        private Random _rand;

        public int[] BestRoute { get; private set; }
        public double BestLength { get; private set; }
        public List<double> BestLengthsHistory { get; private set; }

        public AntColonyOptimization(
            (double x, double y)[] cities,
            int antCount,
            double alpha,
            double beta,
            double evaporation,
            double Q)
        {
            _cities = cities;
            _cityCount = cities.Length;
            _antCount = antCount;

            _alpha = alpha;
            _beta = beta;
            _evaporation = evaporation;
            _Q = Q;

            _rand = new Random();

            _distances = new double[_cityCount, _cityCount];
            _pheromones = new double[_cityCount, _cityCount];
            _eta = new double[_cityCount, _cityCount];

            // Обчислюємо відстані між містами та ініціалізуємо матриці
            for (int i = 0; i < _cityCount; i++)
            {
                for (int j = 0; j < _cityCount; j++)
                {
                    if (i == j)
                    {
                        _distances[i, j] = 0;
                        _pheromones[i, j] = 0.01;
                        _eta[i, j] = 0;
                    }
                    else
                    {
                        double dx = _cities[i].x - _cities[j].x;
                        double dy = _cities[i].y - _cities[j].y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        _distances[i, j] = dist;
                        _pheromones[i, j] = 0.01;
                        _eta[i, j] = 1.0 / dist;
                    }
                }
            }

            BestRoute = null;
            BestLength = double.MaxValue;
            BestLengthsHistory = new List<double>();
        }

        public void Run(int maxIterations)
        {
            for (int iter = 0; iter < maxIterations; iter++)
            {
                var allAntRoutes = new List<int[]>();
                var allAntLengths = new List<double>();

                for (int k = 0; k < _antCount; k++)
                {
                    var route = BuildRoute();
                    double length = RouteLength(route);
                    allAntRoutes.Add(route);
                    allAntLengths.Add(length);

                    if (length < BestLength)
                    {
                        BestLength = length;
                        BestRoute = (int[])route.Clone();
                    }
                }

                EvaporatePheromones();
                UpdatePheromones(allAntRoutes, allAntLengths);
                BestLengthsHistory.Add(BestLength);
            }
        }

        private int[] BuildRoute()
        {
            int startCity = _rand.Next(_cityCount);
            var route = new List<int> { startCity };
            var visited = new HashSet<int> { startCity };

            while (route.Count < _cityCount)
            {
                int current = route.Last();
                int next = SelectNextCity(current, visited);
                route.Add(next);
                visited.Add(next);
            }

            return route.ToArray();
        }

        private int SelectNextCity(int currentCity, HashSet<int> visited)
        {
            double[] probabilities = new double[_cityCount];
            double sum = 0;

            for (int j = 0; j < _cityCount; j++)
            {
                if (!visited.Contains(j))
                {
                    double tau = Math.Pow(_pheromones[currentCity, j], _alpha);
                    double eta = Math.Pow(_eta[currentCity, j], _beta);
                    probabilities[j] = tau * eta;
                    sum += probabilities[j];
                }
                else
                {
                    probabilities[j] = 0;
                }
            }

            double r = _rand.NextDouble() * sum;
            double cum = 0;
            for (int j = 0; j < _cityCount; j++)
            {
                cum += probabilities[j];
                if (cum >= r)
                    return j;
            }

            for (int j = _cityCount - 1; j >= 0; j--)
            {
                if (!visited.Contains(j))
                    return j;
            }
            return 0;
        }

        private void EvaporatePheromones()
        {
            for (int i = 0; i < _cityCount; i++)
            {
                for (int j = 0; j < _cityCount; j++)
                {
                    _pheromones[i, j] *= (1.0 - _evaporation);
                    if (_pheromones[i, j] < 0.0001)
                        _pheromones[i, j] = 0.0001;
                }
            }
        }

        private void UpdatePheromones(List<int[]> routes, List<double> lengths)
        {
            for (int k = 0; k < routes.Count; k++)
            {
                double Lk = lengths[k];
                var route = routes[k];

                for (int i = 0; i < route.Length - 1; i++)
                {
                    int cityA = route[i];
                    int cityB = route[i + 1];
                    _pheromones[cityA, cityB] += _Q / Lk;
                    _pheromones[cityB, cityA] += _Q / Lk;
                }

                int lastCity = route[route.Length - 1];
                int firstCity = route[0];
                _pheromones[lastCity, firstCity] += _Q / Lk;
                _pheromones[firstCity, lastCity] += _Q / Lk;
            }
        }

        private double RouteLength(int[] route)
        {
            double sum = 0;
            for (int i = 0; i < route.Length - 1; i++)
            {
                sum += _distances[route[i], route[i + 1]];
            }
            sum += _distances[route[route.Length - 1], route[0]];
            return sum;
        }
    }
}
