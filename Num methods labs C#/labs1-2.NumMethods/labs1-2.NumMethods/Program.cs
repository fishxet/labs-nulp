using System;
using Newtonsoft.Json;

public class DataContainer
{
    public decimal[,] A { get; set; }
    public decimal[] f { get; set; }
    public decimal[] epsilons { get; set; }
    public decimal[] x { get; set; }
}
class Program
{
    static void Main()
    {
        string json = File.ReadAllText("data.json");
        DataContainer dc = JsonConvert.DeserializeObject<DataContainer>(json);
        Console.WriteLine("======================================================");
        Console.WriteLine(" РОЗВ'ЯЗОК СИСТЕМИ МЕТОДОМ ПРОСТИХ ІТЕРАЦІЙ (матриця A, f)");
        Console.WriteLine("======================================================");

        foreach (var epsilon in dc.epsilons)
        {
            Console.WriteLine($"\nТочність ε = {epsilon}:");
            var result = SimpleIterationMethod(dc.A, dc.f, epsilon, 1000);
            PrintResult("Метод простих ітерацій", result.x, result.iterations, dc.A, dc.f);
        }

        // Розв'язок методом Якобі (компонентний розрахунок)
        Console.WriteLine("\n======================================================");
        Console.WriteLine(" РОЗВ'ЯЗОК СИСТЕМИ МЕТОДОМ ЯКОБІ (поелементний розрахунок)");
        Console.WriteLine("======================================================");

        foreach (var epsilon in dc.epsilons)
        {
            Console.WriteLine($"\nТочність ε = {epsilon}:");
            var result = JacobiMethod(dc.A, dc.f, epsilon, 1000);
            PrintResult("Метод Якобі", result.x, result.iterations, dc.A, dc.f);
        }
    }
    
    static (decimal[] x, int iterations) SimpleIterationMethod(decimal[,] A, decimal[] f, decimal epsilon, int maxIter)
    {
        int n = f.Length;
        decimal[] x = new decimal[n];      // Початкове наближення (нулевий вектор)
        decimal[] xNew = new decimal[n];
        int iterations = 0;
        decimal error = 0;

        // Обчислення вектора c та матриці B
        decimal[,] B = new decimal[n, n];
        decimal[] c = new decimal[n];
        for (int i = 0; i < n; i++)
        {
            if (Math.Abs(A[i, i]) < 1e-15m)
                throw new Exception("Діагональний елемент дуже малий.");
            c[i] = f[i] / A[i, i];
            for (int j = 0; j < n; j++)
            {
                B[i, j] = (i == j) ? 0 : -A[i, j] / A[i, i];
            }
        }

        do
        {
            for (int i = 0; i < n; i++)
            {
                decimal sum = 0;
                for (int j = 0; j < n; j++)
                {
                    sum += B[i, j] * x[j];
                }
                xNew[i] = sum + c[i];
            }

            error = 0;
            for (int i = 0; i < n; i++)
            {
                error = Math.Max(error, Math.Abs(xNew[i] - x[i]));
                x[i] = xNew[i];
            }
            iterations++;
        } while (error > epsilon && iterations < maxIter);

        return (x, iterations);
    }
    
    static (decimal[] x, int iterations) JacobiMethod(decimal[,] A, decimal[] f, decimal epsilon, int maxIter)
    {
        int n = f.Length;
        decimal[] x = new decimal[n];      // Початкове наближення (нулевий вектор)
        decimal[] xNew = new decimal[n];
        int iterations = 0;
        decimal error = 0;

        do
        {
            for (int i = 0; i < n; i++)
            {
                decimal sum = f[i];
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                        sum -= A[i, j] * x[j];
                }
                xNew[i] = sum / A[i, i];
            }
            error = 0;
            for (int i = 0; i < n; i++)
            {
                error = Math.Max(error, Math.Abs(xNew[i] - x[i]));
                x[i] = xNew[i];
            }
            iterations++;
        } while (error > epsilon && iterations < maxIter);

        return (x, iterations);
    }

    // Обчислення вектора нев'язки: r = A*x - f
    static decimal[] CalculateResidual(decimal[,] A, decimal[] x, decimal[] f)
    {
        int n = f.Length;
        decimal[] r = new decimal[n];
        for (int i = 0; i < n; i++)
        {
            decimal sum = 0;
            for (int j = 0; j < n; j++)
            {
                sum += A[i, j] * x[j];
            }
            r[i] = sum - f[i];
        }
        return r;
    }

    // Обчислення евклідової норми вектора
    static decimal VectorNorm(decimal[] vec)
    {
        decimal sum = 0;
        for (int i = 0; i < vec.Length; i++)
        {
            sum += vec[i] * vec[i];
        }
        return (decimal)Math.Sqrt((double)sum);
    }
    
    static void PrintResult(string methodName, decimal[] x, int iterations, decimal[,] A, decimal[] f)
    {
        Console.WriteLine($"{methodName}:");
        Console.WriteLine($"Кількість ітерацій: {iterations}");
        Console.WriteLine("Розв'язок:");
        for (int i = 0; i < x.Length; i++)
        {
            Console.WriteLine($"x[{i + 1}] = {x[i]}");
        }

        decimal[] residual = CalculateResidual(A, x, f);
        Console.WriteLine("\nВектор нев'язки (r = A*x - f):");
        for (int i = 0; i < residual.Length; i++)
        {
            Console.WriteLine($"r[{i + 1}] = {residual[i]}");
        }
        decimal norm = VectorNorm(residual);
        Console.WriteLine($"\nНорма нев'язки: {norm}");
    }
}
