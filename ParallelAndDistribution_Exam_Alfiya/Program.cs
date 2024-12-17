using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter the number (N) <2000 to find a value greater than: ");
        if (!int.TryParse(Console.ReadLine(), out int N) || N >= 2000)
        {
            Console.WriteLine("Invalid input. Please enter a valid integer less than 2000.");
            return;
        }

        Console.WriteLine($"Searching for a number greater than: {N}");
        int arraySize = 100_000_000; 
        int[] numbers = GenerateLargeArray(arraySize);
        Console.WriteLine("Array created. Starting parallel search...");
        Stopwatch stopwatch = Stopwatch.StartNew();
        int? result = FindNumberGreaterThanN(numbers, N);    
        stopwatch.Stop();

        if (result.HasValue)
        {
            Console.WriteLine($"Number found greater than {N}: {result.Value}");
        }
        else
        {
            Console.WriteLine($"No number greater than {N} found in the array.");
        }

        Console.WriteLine($"Search completed in: {stopwatch.Elapsed}");
    }
    static int[] GenerateLargeArray(int size)
    {
        Random random = new Random();
        int[] array = new int[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = random.Next(0, 2000); 
        }
        return array;
    }
       
    static int? FindNumberGreaterThanN(int[] array, int N)
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        int? result = null;

        try
        {
            Parallel.ForEach(
                Partitioner.Create(0, array.Length), 
                new ParallelOptions { CancellationToken = cts.Token },
                (range, state) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        if (array[i] > N)
                        {
                            lock (cts)
                            {
                                if (result == null)
                                {
                                    result = array[i]; 
                                    cts.Cancel();    
                                    state.Stop();      
                                }
                            }
                        }
                    }
                });
        }
        catch (OperationCanceledException)
        {
            // Expected exception 
        }

        return result;
    }
}
