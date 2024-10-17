using System.Collections.Concurrent;

namespace Sudoku;

public static class QuickSorter
{
    public static ConcurrentDictionary<int, int> ThreadIds { get; } = new();

    public static void QuickSort<T>(this IList<T> source) where T : IComparable<T>
    {
        ThreadIds.Clear();

        if (IsSorted(source, 0, source.Count))
            return;

        DoQuickSort(source, 0, source.Count, Environment.ProcessorCount - 1);
    }

    private static bool IsSorted<T>(IList<T> source, int start, int end) where T : IComparable<T>
    {
        for (int i = start; i + 1 < end; i++)
        {
            if (0 != source[i].CompareTo(source[i + 1]))
                return false;
        }
        return true;
    }

    private static void DoQuickSort<T>(IList<T> source, int start, int end, int threads)
        where T : IComparable<T>
    {
        ThreadIds.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, _ => 1, (k, v) => v+1);
        if (start + 1 >= end) return;

        if (end - start < 1_000)
            if (IsSorted(source, start, end))
                return;

        var putToFirst = Random.Shared.Next(start, end);
        (source[start], source[putToFirst]) = (source[putToFirst], source[start]);

        var head = start;
        var tail = end - 1;
        while (head < tail)
        {
            var comparison = source[head].CompareTo(source[head + 1]);
            if (comparison <= 0)
            {
                (source[head + 1], source[tail]) = (source[tail], source[head + 1]);
                tail--;
            }
            else
            {
                (source[head], source[head + 1]) = (source[head + 1], source[head]);
                head++;
            }
        }

        if (threads > 0)
        {
            Parallel.Invoke(
                () => DoQuickSort<T>(source, start, head, threads - 1),
                () => DoQuickSort<T>(source, head, end, threads - 1)
            );
        }
        else
        {
            DoQuickSort<T>(source, start, head, threads);
            DoQuickSort<T>(source, head, end, threads);
        }
    }
}