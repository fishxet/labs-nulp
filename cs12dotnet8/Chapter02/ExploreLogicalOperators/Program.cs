namespace ExploreLogicalOperators;

class Program
{
    static void Main(string[] args)
    {
        int x = 10, y = 6;
        Console.WriteLine("Expression      |      Decimal   |      Binary");
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine($"x               |   {x,10}   |    {x:B8}");
        Console.WriteLine($"y               |   {y,10}   |    {y:B8}");
        Console.WriteLine($"x&y             |   {x&y,10}   |    {x&y:B8}");
        Console.WriteLine($"x|y             |   {x|y,10}   |    {x|y:B8}");
        Console.WriteLine($"x^y             |   {x^y,10}   |    {x^y:B8}");
        Console.WriteLine($"x<<3            |   {x<<3,10}   |    {x<<3:B8}");
        Console.WriteLine($"x>>3            |   {x>>3,10}   |    {x>>3:B8}");
    }
}