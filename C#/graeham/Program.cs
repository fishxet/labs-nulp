using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int[] array = { 10, 5, 2, 3 };
        int[] sortedArray = QuickSort(array);
        Console.WriteLine(string.Join(", ", sortedArray));
    }

    static int[] QuickSort(int[] array)
    {
        if (array.Length < 2)
        {
            return array;
        }

        int pivot = array[0];
        List<int> less = new List<int>();
        List<int> greater = new List<int>();

        for (int i = 1; i < array.Length; i++)
        {
            if (array[i] <= pivot)
            {
                less.Add(array[i]);
            }
            else
            {
                greater.Add(array[i]);
            }
        }

        int[] sortedLess = QuickSort(less.ToArray());
        int[] sortedGreater = QuickSort(greater.ToArray());

        return CombineArrays(sortedLess, pivot, sortedGreater);
    }

    static int[] CombineArrays(int[] less, int pivot, int[] greater)
    {
        int[] result = new int[less.Length + 1 + greater.Length];
        less.CopyTo(result, 0);
        result[less.Length] = pivot;
        greater.CopyTo(result, less.Length + 1);
        return result;
    }
}