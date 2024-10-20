using System.Collections.Concurrent;

namespace Sudoku;

public static class QuickSorter
{
    public static ConcurrentDictionary<int, int> ThreadIds { get; } = new();

    public static void QuickSort<T>(this IList<T> source, bool enableParallel = true, bool withRecursion = true)
        where T : IComparable<T>
    {
        ThreadIds.Clear();

        if (IsSorted(source, 0, source.Count))
            return;

        var threadsCount = enableParallel ? Environment.ProcessorCount - 1 : -1_000;
        if (withRecursion)
            DoQuickSortRecursion(source, 0, source.Count, threadsCount);
        else if (!enableParallel)
            DoQuickSortStack(source, 0, source.Count);
        else
            DoQuickSortStackParallel(source, 0, source.Count).GetAwaiter().GetResult();
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

    private static void DoQuickSortRecursion<T>(IList<T> source, int start, int end, int threads)
        where T : IComparable<T>
    {
        ThreadIds.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, _ => 1, (k, v) => v + 1);
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
                () => DoQuickSortRecursion<T>(source, start, head, threads - 1),
                () => DoQuickSortRecursion<T>(source, head, end, threads - 1)
            );
        }
        else
        {
            DoQuickSortRecursion<T>(source, start, head, threads);
            DoQuickSortRecursion<T>(source, head, end, threads);
        }
    }

    private static void DoQuickSortStack<T>(IList<T> source, int start, int end)
        where T : IComparable<T>
    {
        var slices = new Stack<(int s, int e)>();
        slices.Push((start, end));
        while (slices.Any())
        {
            var (s, e) = slices.Pop();
            if (s + 1 >= e) continue;
            if (e - s < 1_000)
                if (IsSorted(source, s, e))
                    continue;

            var putToFirst = Random.Shared.Next(s, e);
            (source[s], source[putToFirst]) = (source[putToFirst], source[s]);

            var head = s;
            var tail = e - 1;
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
            slices.Push((s, head));
            slices.Push((head, e));
        }
    }

    //too slow
    private static async Task DoQuickSortStackParallel<T>(IList<T> source, int start, int end)
        where T : IComparable<T>
    {
        var slices = new ConcurrentStack<(int s, int e)>();
        slices.Push((start, end));

        var tasks = new List<Task>() { Task.Run(ProcessSlice) };

        while (tasks.Any())
        {
            var current = await Task.WhenAny(tasks);
            tasks.Remove(current);

            if (tasks.Count * 2.5 < Environment.ProcessorCount - 1)
            {
                //as each ProcessSlice call may add by 2 slices into the stack, trigger by 2 tasks at once
                if (!slices.IsEmpty)
                    tasks.Add(Task.Run(ProcessSlice));
                if (!slices.IsEmpty)
                    tasks.Add(Task.Run(ProcessSlice));
            }
            else
            {
                if (!slices.IsEmpty)
                    ProcessSlice();
            }
        }


        void ProcessSlice()
        {
            ThreadIds.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, _ => 1, (k, v) => v + 1);
            if (!slices.TryPop(out var slice))
                return;

            var (s, e) = slice;

            if (s + 1 >= e) return;
            if (e - s < 1_000)
                if (IsSorted(source, s, e))
                    return;

            var putToFirst = Random.Shared.Next(s, e);
            (source[s], source[putToFirst]) = (source[putToFirst], source[s]);

            var head = s;
            var tail = e - 1;
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

            if (s + 1 < head)
                slices.Push((s, head));

            if (head + 1 < e)
                slices.Push((head, e));
        }
    }
}