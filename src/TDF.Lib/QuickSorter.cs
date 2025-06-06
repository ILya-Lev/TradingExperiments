﻿using System.Collections.Concurrent;

namespace TDF.Lib;

public interface IQuickSorter<T>
{
    Task<T[]> Sort(T[] array);
}

public class QuickSorter<T>(Func<T, T, bool> isBefore) : IQuickSorter<T>
{
    public Task<T[]> Sort(T[] array)
    {
        if (IsSorted(array))
            return Task.FromResult(array);

        var states = new Stack<(int start, int end)>();
        states.Push((0, array.Length - 1));

        while (states.Any())
        {
            var (start, end) = states.Pop();
            var middle = PutOneItemInItsFinalPosition(array, start, end);

            if (middle - 1 > start) states.Push((start, middle - 1));
            if (end > middle + 1) states.Push((middle + 1, end));
        }

        return Task.FromResult(array);
    }

    private bool IsSorted(T[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (isBefore(array[i], array[i - 1]))
                return false;
        }
        return true;
    }

    private int PutOneItemInItsFinalPosition(T[] array, int start, int end)
    {
        var seed = Random.Shared.Next(start, end + 1);
        Swap(array, start, seed);

        var head = start;
        var tail = end;
        while (head < tail)
        {
            if (isBefore(array[head], array[head + 1]))
            {
                Swap(array, head + 1, tail);
                tail--;
            }
            else
            {
                Swap(array, head, head + 1);
                head++;
            }
        }

        return head;
    }

    private static void Swap(T[] array, int lhs, int rhs) => (array[lhs], array[rhs]) = (array[rhs], array[lhs]);
}

public class QuickSorterAsync<T>(Func<T, T, bool> isBefore) : IQuickSorter<T>
{
    public async Task<T[]> Sort(T[] array)
    {
        if (IsSorted(array))
            return array;

        var task = Task.Factory
            .StartNew(() => DoSort(array, 0, array.Length - 1, Environment.ProcessorCount)
            , CancellationToken.None
            , TaskCreationOptions.None
            , TaskScheduler.Default);

        await task;
        return array;
    }

    private void DoSort(T[] array, int start, int end, int threads)
    {
        var middle = PutOneItemInItsFinalPosition(array, start, end);

        if (middle - 1 > start)
        {
            if (threads > 1)
                Task.Factory.StartNew(() => DoSort(array, start, middle - 1, threads - 1)
                    , CancellationToken.None
                    , TaskCreationOptions.AttachedToParent
                    , TaskScheduler.Default);
            else
                DoSort(array, start, middle - 1, threads);
        }

        if (end > middle + 1)
        {
            if (threads > 1)
                Task.Factory.StartNew(() => DoSort(array, middle + 1, end, threads - 1)
                    , CancellationToken.None
                    , TaskCreationOptions.AttachedToParent
                    , TaskScheduler.Default);
            else
                DoSort(array, middle + 1, end, threads);
        }
    }

    private bool IsSorted(T[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (isBefore(array[i], array[i - 1]))
                return false;
        }
        return true;
    }

    private int PutOneItemInItsFinalPosition(T[] array, int start, int end)
    {
        var seed = Random.Shared.Next(start, end + 1);
        Swap(array, start, seed);

        var head = start;
        var tail = end;
        while (head < tail)
        {
            if (isBefore(array[head], array[head + 1]))
            {
                Swap(array, head + 1, tail);
                tail--;
            }
            else
            {
                Swap(array, head, head + 1);
                head++;
            }
        }

        return head;
    }

    private static void Swap(T[] array, int lhs, int rhs) => (array[lhs], array[rhs]) = (array[rhs], array[lhs]);
}

public class QuickSorterParallel<T>(Func<T, T, bool> isBefore) : IQuickSorter<T>
{
    public async Task<T[]> Sort(T[] array)
    {
        await Task.Yield();

        if (!IsSorted(array))
            DoSort(array, 0, array.Length - 1, Environment.ProcessorCount);

        return array;
    }
    private void DoSort(T[] array, int start, int end, int threads)
    {
        var middle = PutOneItemInItsFinalPosition(array, start, end);

        if (threads > 2)
        {
            var funcs = new List<Action>();
            if (middle - 1 > start) funcs.Add(() => DoSort(array, start, middle - 1, threads - 1));
            if (end > middle + 1) funcs.Add(() => DoSort(array, middle + 1, end, threads - 2));
            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = threads }, funcs.ToArray());
        }
        else
        {
            if (middle - 1 > start) DoSort(array, start, middle - 1, threads - 1);
            if (end > middle + 1) DoSort(array, middle + 1, end, threads - 2);
        }
    }

    private bool IsSorted(T[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (isBefore(array[i], array[i - 1]))
                return false;
        }
        return true;
    }

    private int PutOneItemInItsFinalPosition(T[] array, int start, int end)
    {
        var seed = Random.Shared.Next(start, end + 1);
        Swap(array, start, seed);

        var head = start;
        var tail = end;
        while (head < tail)
        {
            if (isBefore(array[head], array[head + 1]))
            {
                Swap(array, head + 1, tail);
                tail--;
            }
            else
            {
                Swap(array, head, head + 1);
                head++;
            }
        }

        return head;
    }

    private static void Swap(T[] array, int lhs, int rhs) => (array[lhs], array[rhs]) = (array[rhs], array[lhs]);
}

public class QuickSorterParallelStack<T>(Func<T, T, bool> isBefore) : IQuickSorter<T>
{
    public Task<T[]> Sort(T[] array)
    {
        if (IsSorted(array))
            return Task.FromResult(array);

        var m = PutOneItemInItsFinalPosition(array, 0, array.Length - 1);

        var states = new ConcurrentStack<(int start, int middle, int end)>();
        states.Push((0, m, array.Length - 1));

        var threads = Environment.ProcessorCount;
        while (states.TryPop(out var state))
        {
            var (start, middle, end) = state;

            if (threads > 1)
                Parallel.Invoke(new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Math.Max(1, threads)
                },
                    () =>
                    {
                        Interlocked.Decrement(ref threads);
                        if (middle - 1 <= start) return;
                        var m1 = PutOneItemInItsFinalPosition(array, start, middle - 1);
                        states.Push((start, m1, middle - 1));
                        Interlocked.Increment(ref threads);
                    },
                    () =>
                    {
                        Interlocked.Decrement(ref threads);
                        if (end <= middle + 1) return;
                        var m2 = PutOneItemInItsFinalPosition(array, middle + 1, end);
                        states.Push((middle + 1, m2, end));
                        Interlocked.Increment(ref threads);
                    });
            else
            {
                if (middle - 1 > start)
                {
                    var m1 = PutOneItemInItsFinalPosition(array, start, middle - 1);
                    states.Push((start, m1, middle - 1));
                }

                if (end > middle + 1)
                {
                    var m2 = PutOneItemInItsFinalPosition(array, middle + 1, end);
                    states.Push((middle + 1, m2, end));
                }
            }
        }

        return Task.FromResult(array);
    }

    private bool IsSorted(T[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (isBefore(array[i], array[i - 1]))
                return false;
        }
        return true;
    }

    private int PutOneItemInItsFinalPosition(T[] array, int start, int end)
    {
        var seed = Random.Shared.Next(start, end + 1);
        Swap(array, start, seed);

        var head = start;
        var tail = end;
        while (head < tail)
        {
            if (isBefore(array[head], array[head + 1]))
            {
                Swap(array, head + 1, tail);
                tail--;
            }
            else
            {
                Swap(array, head, head + 1);
                head++;
            }
        }

        return head;
    }

    private static void Swap(T[] array, int lhs, int rhs) => (array[lhs], array[rhs]) = (array[rhs], array[lhs]);
}
