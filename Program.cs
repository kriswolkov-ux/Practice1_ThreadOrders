using System;
using System.Threading;

public class Program
{
    private const int ThreadCount = 8;
    private const int IncrementsPerThread = 100_000;
    private static int completedOrders = 0;

    public static void Main()
    {
        int expected = ThreadCount * IncrementsPerThread;

        int unsafeCount = RunUnsafe();
        int safeCount = RunSafe();

        Console.WriteLine("=== Обычный инкремент ===");
        Console.WriteLine($"Ожидалось: {expected}");
        Console.WriteLine($"Получено: {unsafeCount}");
        Console.WriteLine($"Потеряно: {expected - unsafeCount}");
        Console.WriteLine();

        Console.WriteLine("=== Interlocked.Increment ===");
        Console.WriteLine($"Ожидалось: {expected}");
        Console.WriteLine($"Получено: {safeCount}");
        Console.WriteLine($"Потеряно: {expected - safeCount}");
    }

    private static int RunUnsafe()
    {
        completedOrders = 0;
        Thread[] threads = CreateThreads(() =>
        {
            for (int j = 0; j < IncrementsPerThread; j++)
            {
                // Обычный инкремент: чтение, сложение и запись не атомарны.
                completedOrders++;
            }
        });

        StartAndJoin(threads);
        return completedOrders;
    }

    private static int RunSafe()
    {
        completedOrders = 0;
        Thread[] threads = CreateThreads(() =>
        {
            for (int j = 0; j < IncrementsPerThread; j++)
            {
                Interlocked.Increment(ref completedOrders);
            }
        });

        StartAndJoin(threads);
        return completedOrders;
    }

    private static Thread[] CreateThreads(ThreadStart body)
    {
        Thread[] threads = new Thread[ThreadCount];
        for (int i = 0; i < ThreadCount; i++)
        {
            threads[i] = new Thread(body);
        }

        return threads;
    }

    private static void StartAndJoin(Thread[] threads)
    {
        foreach (Thread thread in threads)
        {
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }
}
