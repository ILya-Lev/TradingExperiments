namespace Sudoku;

internal static class QuickSorter001
{
    public static IReadOnlyList<T> Sort<T>(IEnumerable<T> source) where T : IComparable<T>
    {
        var s = source.ToArray();
        var boundaries = new Stack<(int start, int end)>(CalculateCapacity(s));
        boundaries.Push((0, s.Length - 1));

        while (boundaries.Any())
        {
            var (start, end) = boundaries.Pop();
            if (start >= end) continue;

            var head = TraverseRange(s, start, end);

            boundaries.Push((start, head));
            boundaries.Push((head + 1, end));
        }

        return s;
    }

    //todo: make it parallel via task-based parallelism
    public static IList<T> SortParallel<T>(IList<T> source) where T : IComparable<T>
    {
        var s = source.ToArray();
        SortRecursively(s, 0, s.Length - 1, Environment.ProcessorCount - 1);
        return s;
    }

    private static void SortRecursively<T>(T[] s, int start, int end, int availableThreads)
        where T : IComparable<T>
    {
        if (start >= end) return;

        var head = TraverseRange(s, start, end);

        if (availableThreads > 0)
        {
            Parallel.Invoke
            ([
                () => { SortRecursively<T>(s, start, head, availableThreads - 1); },
                () => { SortRecursively<T>(s, head+1, end, availableThreads - 1); }
            ]);
        }
        else
        {
            SortRecursively<T>(s, start, head, availableThreads - 1);
            SortRecursively<T>(s, head + 1, end, availableThreads - 1);
        }
    }

    private static int CalculateCapacity<T>(T[] s)
        => (int)(
            2 * s.Length //*2 as we go over the collection 2 times on each iteration
              * Math.Log2(s.Length) //on each iteration we split the sequence in 2
            + 1); //+1 for Ceiling, not precise but good enough

    private static int TraverseRange<T>(T[] s, int start, int end) where T : IComparable<T>
    {
        var seedIndex = Random.Shared.Next(start, end);
        var middle = (start + end) / 2;
        (s[middle], s[seedIndex]) = (s[seedIndex], s[middle]);//swap
        var pivot = s[middle];

        int head = start - 1, tail = end + 1;
        while (head < tail)
        {

            do { head++; } while (s[head].CompareTo(pivot) < 0);

            do { tail--; } while (s[tail].CompareTo(pivot) > 0);

            if (head >= tail) return tail;

            (s[tail], s[head]) = (s[head], s[tail]);
        }

        return head;
    }

    private static bool HasConverged<T>(IReadOnlyList<T> s, int start, int end)
        where T : IComparable<T>
    {
        for (var i = start; i + 1 < end; i++)
        {
            var comparisonResult = s[i].CompareTo(s[i + 1]);
            if (comparisonResult > 0) return false;
        }

        return true;
    }
}