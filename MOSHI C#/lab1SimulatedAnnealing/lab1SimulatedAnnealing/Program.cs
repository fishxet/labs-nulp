using System;
using System.Diagnostics;

namespace NQueensSimulatedAnnealing
{
    class Program
    {
        static int N = 8; // Розмір шахової дошки (N x N)
        static Random random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("=== Лабораторна робота №1: Алгоритм відпалу ===");

            // Реалізація алгоритму відпалу
            Console.WriteLine("\n[1] Алгоритм відпалу:");
            var (saSolution, saTime, saIterations) = SolveWithSimulatedAnnealing();
            Console.WriteLine($"Результат після {saIterations} ітерацій");
            PrintBoard(saSolution);
            Console.WriteLine($"Час: {saTime} мс | Ітерації: {saIterations} | Конфлікти: {CalculateConflicts(saSolution)}");

            // Порівняння з випадковим пошуком
            Console.WriteLine("\n[2] Випадковий пошук (для порівняння):");
            var (rsSolution, rsTime, rsIterations) = SolveWithRandomSearch();
            Console.WriteLine($"Результат після {rsIterations} ітерацій");
            PrintBoard(rsSolution);
            Console.WriteLine($"Час: {rsTime} мс | Ітерації: {rsIterations} | Конфлікти: {CalculateConflicts(rsSolution)}");

            // Порівняння з "жадібним" алгоритмом
            Console.WriteLine("\n[3] Жадібний алгоритм (додатково):");
            var (gaSolution, gaTime, gaIterations) = SolveWithGreedyAlgorithm();
            Console.WriteLine($"Результат після {gaIterations} ітерацій");
            PrintBoard(gaSolution);
            Console.WriteLine($"Час: {gaTime} мс | Ітерації: {gaIterations} | Конфлікти: {CalculateConflicts(gaSolution)}");
        }

        // 1. Алгоритм відпалу
        static (int[] solution, long time, int iterations) SolveWithSimulatedAnnealing()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int iterations = 0;
            int[] queens = InitializeQueens();
            Console.WriteLine("Стартова дошка Алгоритму відпалу: ");
            PrintBoard(queens);
            int conflicts = CalculateConflicts(queens);
            double temperature = 1000.0;
            double coolingRate = 0.999;

            while (temperature > 0.1 && conflicts > 0)
            {
                int[] newQueens = (int[])queens.Clone();
                int column = random.Next(N);
                newQueens[column] = random.Next(N);

                int newConflicts = CalculateConflicts(newQueens);
                int delta = newConflicts - conflicts;

                if (delta < 0 || Math.Exp(-delta / temperature) > random.NextDouble())
                {
                    queens = newQueens;
                    conflicts = newConflicts;
                }

                temperature *= coolingRate;
                iterations++;
            }

            sw.Stop();
            return (queens, sw.ElapsedMilliseconds, iterations);
        }

        // 2. Випадковий пошук (для порівняння)
        static (int[] solution, long time, int iterations) SolveWithRandomSearch()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int iterations = 0;
            int maxIterations = 100000;
            int[] bestQueens = InitializeQueens();
            Console.WriteLine("Стартова дошка Алгоритму випадкового пошуку: ");
            PrintBoard(bestQueens);
            int bestConflicts = CalculateConflicts(bestQueens);

            while (bestConflicts > 0 && iterations < maxIterations)
            {
                int[] newQueens = (int[])bestQueens.Clone();
                int column = random.Next(N);
                newQueens[column] = random.Next(N);

                int newConflicts = CalculateConflicts(newQueens);
                if (newConflicts < bestConflicts)
                {
                    bestQueens = newQueens;
                    bestConflicts = newConflicts;
                }
                iterations++;
            }

            sw.Stop();
            return (bestQueens, sw.ElapsedMilliseconds, iterations);
        }

        // 3. Жадібний алгоритм (додатково)
        static (int[] solution, long time, int iterations) SolveWithGreedyAlgorithm()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int iterations = 0;
            int[] queens = InitializeQueens();
            Console.WriteLine("Стартова дошка жадібного алгоритму: ");
            PrintBoard(queens);
            while (CalculateConflicts(queens) > 0 || sw.ElapsedMilliseconds < 4000)
            {
                if (sw.ElapsedMilliseconds > 4000) {
                    break;
                }
                int worstColumn = FindWorstColumn(queens);
                int bestRow = FindBestRow(queens, worstColumn);
                queens[worstColumn] = bestRow;
                iterations++;
            }
            if (sw.ElapsedMilliseconds > 4000)
            {
                Console.WriteLine("Алгоритм не впорався за задану к-сть часу.");
            }
            sw.Stop();
            return (queens, sw.ElapsedMilliseconds - 1, iterations);
        }

        // Допоміжні функції
        static int[] InitializeQueens()
        {
            int[] queens = new int[N];
            for (int i = 0; i < N; i++)
                queens[i] = random.Next(N);
            return queens;
        }

        static int CalculateConflicts(int[] queens)
        {
        int conflicts = 0;
        for (int i = 0; i < N; i++)
        {
            for (int j = i + 1; j < N; j++)
            {
                // Горизонтальний конфлікт (однаковий рядок)
                if (queens[i] == queens[j])
                    conflicts++;

                // Діагональний конфлікт
                if (Math.Abs(i - j) == Math.Abs(queens[i] - queens[j]))
                    conflicts++;
            }
        }
        return conflicts;
        }

        static int FindWorstColumn(int[] queens)
        {
            int maxConflicts = -1;
            int worstColumn = 0;
            for (int i = 0; i < N; i++)
            {
                int currentConflicts = 0;
                for (int j = 0; j < N; j++)
                    if (i != j && Math.Abs(i - j) == Math.Abs(queens[i] - queens[j]))
                        currentConflicts++;
                if (currentConflicts > maxConflicts)
                {
                    maxConflicts = currentConflicts;
                    worstColumn = i;
                }
            }
            return worstColumn;
        }

        static int FindBestRow(int[] queens, int column)
        {
            int minConflicts = int.MaxValue;
            int bestRow = queens[column];
            for (int row = 0; row < N; row++)
            {
                queens[column] = row;
                int conflicts = CalculateConflicts(queens);
                if (conflicts < minConflicts)
                {
                    minConflicts = conflicts;
                    bestRow = row;
                }
            }
            return bestRow;
        }

        static void PrintBoard(int[] queens)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                    Console.Write(queens[j] == i ? "Q " : ". ");
                Console.WriteLine();
            }
            Console.WriteLine("");
        }
    }
}