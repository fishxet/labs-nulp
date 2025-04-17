using System;

namespace TicTacToe
{
    class Program
    {
        // Ігрове поле (3x3)
        private static char[,] _board = new char[3, 3];
        // Символи гравця (X) та комп’ютера (O)
        private static char _playerSymbol = 'X';
        private static char _computerSymbol = 'O';

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Підтримка кирилиці
            InitializeBoard(); // Ініціалізація поля
            bool isPlayerTurn = true; // Чи ход гравця

            // Головний цикл гри
            while (true)
            {
                PrintBoard(); // Вивід поля
                if (isPlayerTurn)
                {
                    PlayerMove(); // Хід гравця
                }
                else
                {
                    ComputerMove(); // Хід комп’ютера
                }

                // Перевірка на перемогу
                if (CheckWin(_playerSymbol))
                {
                    PrintBoard();
                    Console.WriteLine("Ви перемогли! 🎉");
                    break;
                }
                else if (CheckWin(_computerSymbol))
                {
                    PrintBoard();
                    Console.WriteLine("Комп’ютер переміг... 😢");
                    break;
                }
                else if (IsBoardFull())
                {
                    PrintBoard();
                    Console.WriteLine("Нічия! 🤝");
                    break;
                }

                isPlayerTurn = !isPlayerTurn; // Зміна ходу
            }
        }

        // Ініціалізація поля (пусті клітинки)
        private static void InitializeBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _board[i, j] = ' ';
                }
            }
        }

        // Вивід поля у консоль
        private static void PrintBoard()
        {
            Console.Clear();
            Console.WriteLine("  1 2 3");
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"{i + 1} ");
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(_board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        // Хід гравця
        private static void PlayerMove()
        {
            while (true)
            {
                Console.WriteLine("Ваш хід (рядок та стовпець, наприклад: 1 2): ");
                string[] input = Console.ReadLine().Split();
                if (input.Length != 2)
                {
                    Console.WriteLine("Невірний формат!");
                    continue;
                }

                int row = int.Parse(input[0]) - 1;
                int col = int.Parse(input[1]) - 1;

                if (row < 0 || row > 2 || col < 0 || col > 2)
                {
                    Console.WriteLine("Позиція поза межами поля!");
                    continue;
                }

                if (_board[row, col] == ' ')
                {
                    _board[row, col] = _playerSymbol;
                    break;
                }
                else
                {
                    Console.WriteLine("Клітинка вже зайнята!");
                }
            }
        }

        // Хід комп’ютера (простий алгоритм з вибором виграшного ходу)
        private static void ComputerMove()
        {
            // Спроба знайти виграшний хід
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_board[i, j] == ' ')
                    {
                        _board[i, j] = _computerSymbol;
                        if (CheckWin(_computerSymbol))
                        {
                            return;
                        }
                        _board[i, j] = ' ';
                    }
                }
            }

            // Випадковий хід, якщо виграшний не знайдено
            Random random = new Random();
            int row, col;
            do
            {
                row = random.Next(0, 3);
                col = random.Next(0, 3);
            } while (_board[row, col] != ' ');

            _board[row, col] = _computerSymbol;
        }

        // Перевірка перемоги
        private static bool CheckWin(char symbol)
        {
            // Перевірка рядків і стовпців
            for (int i = 0; i < 3; i++)
            {
                if (_board[i, 0] == symbol && _board[i, 1] == symbol && _board[i, 2] == symbol) return true;
                if (_board[0, i] == symbol && _board[1, i] == symbol && _board[2, i] == symbol) return true;
            }

            // Перевірка діагоналей
            if (_board[0, 0] == symbol && _board[1, 1] == symbol && _board[2, 2] == symbol) return true;
            if (_board[0, 2] == symbol && _board[1, 1] == symbol && _board[2, 0] == symbol) return true;

            return false;
        }

        // Перевірка на заповненість поля
        private static bool IsBoardFull()
        {
            foreach (char cell in _board)
            {
                if (cell == ' ') return false;
            }
            return true;
        }
    }
}