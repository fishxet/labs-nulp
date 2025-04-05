using System.Diagnostics;

namespace AboutSizeCOnsole;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("|---------------------------|");
        Console.WriteLine("|Type        |        sizeof|");
        Console.WriteLine("|---------------------------|");
        Console.WriteLine($"|Int         |{sizeof(int), 14}|");
        Console.WriteLine($"|UInt        |{sizeof(uint), 14}|");
        Console.WriteLine($"|Float:      |{sizeof(float),14}|");
        Console.WriteLine($"|Double      |{sizeof(double), 14}|");
        Console.WriteLine($"|Long        |{sizeof(long), 14}|");
        Console.WriteLine($"|ULong       |{sizeof(ulong), 14}|");
        Console.WriteLine($"|Decimal     |{sizeof(decimal),14}|");
        Console.WriteLine("|---------------------------|");

    }
}
