using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

namespace ClusteringComparison
{
    public class Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        // Виправлено CS8765: додано nullable тип
        public override bool Equals(object? obj) => 
            obj is Point other && Math.Abs(X - other.X) < 1e-9 && Math.Abs(Y - other.Y) < 1e-9;

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public static class RandomExtensions
    {
        public static double NextGaussian(this Random rand, double mean = 0, double stdDev = 1)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            return mean + stdDev * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }

    public class Cluster
    {
        // Виправлено CS0200: додано setter
        public List<Point> Points { get; set; } = new List<Point>();
        
        // Виправлено CS8618: ініціалізація значення
        public Point Centroid { get; set; } = new Point(0, 0);
    }

    public static class Distance
    {
        public static double Euclidean(Point a, Point b) => 
            Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }

    public class KMeans
    {
        public int K { get; set; } = 3;
        public int MaxIterations { get; set; } = 100;

        public List<Cluster> Cluster(List<Point> data)
        {
            var rand = new Random();
            var centroids = data.OrderBy(_ => rand.Next()).Take(K).ToList();
            var clusters = new List<Cluster>();

            for (var iter = 0; iter < MaxIterations; iter++)
            {
                clusters = centroids.Select(c => new Cluster { Centroid = c }).ToList();
                
                foreach (var point in data)
                {
                    var nearestCluster = clusters
                        .Select((cluster, index) => (Index: index, Distance: Distance.Euclidean(point, cluster.Centroid)))
                        .OrderBy(x => x.Distance)
                        .First().Index;
                    
                    clusters[nearestCluster].Points.Add(point);
                }

                var newCentroids = clusters.Select(cluster => 
                    cluster.Points.Count == 0 
                        ? cluster.Centroid 
                        : new Point(
                            cluster.Points.Average(p => p.X), 
                            cluster.Points.Average(p => p.Y))
                ).ToList();

                if (centroids.SequenceEqual(newCentroids)) break;
                centroids = newCentroids;
            }

            return clusters;
        }
    }

    public class DBSCAN
    {
        public double Eps { get; set; } = 0.1;
        public int MinPts { get; set; } = 10;

        public List<Cluster> Cluster(List<Point> data)
        {
            var clusters = new List<Cluster>();
            var visited = new HashSet<Point>();

            foreach (var point in data)
            {
                if (visited.Contains(point)) continue;
                visited.Add(point);

                var neighbors = GetNeighbors(point, data);
                if (neighbors.Count < MinPts) continue;

                var cluster = new Cluster();
                var queue = new Queue<Point>();
                queue.Enqueue(point);
                cluster.Points.Add(point);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    var currentNeighbors = GetNeighbors(current, data);

                    if (currentNeighbors.Count < MinPts) continue;
                    
                    foreach (var neighbor in currentNeighbors.Where(neighbor => !visited.Contains(neighbor)))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        cluster.Points.Add(neighbor);
                    }
                }
                
                clusters.Add(cluster);
            }

            var noise = data.Where(p => !clusters.Any(c => c.Points.Contains(p))).ToList();
            if (noise.Count > 0) clusters.Add(new Cluster { Points = noise });

            return clusters;
        }

        private List<Point> GetNeighbors(Point point, List<Point> data) => 
            data.Where(p => Distance.Euclidean(point, p) <= Eps).ToList();
    }

    public static class Program
    {
        public static List<Point> GenerateTestData(int n)
        {
            var rand = new Random();
            var points = new List<Point>();
            
            for (var i = 0; i < n; i++)
            {
                double x, y;
                switch (i % 3)
                {
                    case 0:
                        x = Math.Clamp(0.3 + rand.NextGaussian(0, 0.1), 0, 1);
                        y = Math.Clamp(0.3 + rand.NextGaussian(0, 0.1), 0, 1);
                        break;
                    case 1:
                        x = Math.Clamp(0.7 + rand.NextGaussian(0, 0.1), 0, 1);
                        y = Math.Clamp(0.3 + rand.NextGaussian(0, 0.1), 0, 1);
                        break;
                    default:
                        x = Math.Clamp(0.5 + rand.NextGaussian(0, 0.1), 0, 1);
                        y = Math.Clamp(0.7 + rand.NextGaussian(0, 0.1), 0, 1);
                        break;
                }
                points.Add(new Point(x, y));
            }
            
            return points;
        }

        public static void PlotClusters(List<Cluster> clusters, string fileName)
        {
            var plotModel = new PlotModel { Title = fileName.Split('.')[0] };
            var colors = new[] { OxyColors.Red, OxyColors.Blue, OxyColors.Green, OxyColors.Orange };

            for (var i = 0; i < clusters.Count; i++)
            {
                var series = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = colors[i % colors.Length]
                };
                
                series.Points.AddRange(clusters[i].Points.Select(p => 
                    new ScatterPoint(p.X, p.Y)));
                plotModel.Series.Add(series);
            }

            using var stream = File.Create(fileName);
    var exporter = new PngExporter { Width = 800, Height = 800 };
    exporter.Export(plotModel, stream);
        }

        // Виправлено CS1501: видалено зайвий аргумент
        public static double CalculateWCSS(List<Cluster> clusters) =>
            clusters.Sum(cluster => 
                cluster.Points.Sum(point => 
                    Math.Pow(Distance.Euclidean(
                        point, 
                        new Point(cluster.Points.Average(p => p.X), cluster.Points.Average(p => p.Y))
                    ), 2)));

        static void Main()
        {
            var data = GenerateTestData(1000);
            
            var kmeans = new KMeans();
            var kmeansClusters = kmeans.Cluster(data);
            PlotClusters(kmeansClusters, "kmeans_clusters.png");
            Console.WriteLine($"K-Means Clusters: {kmeansClusters.Count}, WCSS: {CalculateWCSS(kmeansClusters):F2}");
            
            var dbscan = new DBSCAN();
            var dbscanClusters = dbscan.Cluster(data);
            PlotClusters(dbscanClusters, "dbscan_clusters.png");
            Console.WriteLine($"DBSCAN Clusters: {dbscanClusters.Count}, WCSS: {CalculateWCSS(dbscanClusters):F2}");
        }
    }
}