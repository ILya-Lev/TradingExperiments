namespace Sudoku;

internal static class QuickSorter001
{
    public static IReadOnlyList<T> Sort<T>(IEnumerable<T> source) where T : IComparable<T>
    {
        var s = source.ToArray();
        var boundaries = new Stack<(int start, int end)>();
        boundaries.Push((0, s.Length));

        while (boundaries.Any())
        {
            var (start, end) = boundaries.Pop();
            var seedIndex = Random.Shared.Next(start, end);
            (s[start], s[seedIndex]) = (s[seedIndex], s[start]);//swap

            var head = TraverseRange(s, start, end);

            if (!HasConverged(s, start, head)) boundaries.Push((start, head));
            if (!HasConverged(s, head, end)) boundaries.Push((head, end));
        }

        return s;
    }

    //todo: make it parallel via task-based parallelism
    public static IList<T> SortParallel<T>(IList<T> source) where T : IComparable<T>
    {
        var s = source.ToArray();
        SortRecursively(s, 0, s.Length).Wait();
        return s;
    }

    private static Task SortRecursively<T>(T[] s, int start, int end) where T : IComparable<T>
    {
        var seedIndex = Random.Shared.Next(start, end);
        (s[start], s[seedIndex]) = (s[seedIndex], s[start]);//swap

        var head = TraverseRange(s, start, end);

        var tasks = new[] { Task.CompletedTask, Task.CompletedTask };
        if (!HasConverged(s, start, head)) tasks[0] = SortRecursively<T>(s, start, head);
        if (!HasConverged(s, head, end)) tasks[1] = SortRecursively<T>(s, head, end);
        return Task.WhenAll(tasks);
    }

    private static int TraverseRange<T>(T[] s, int start, int end) where T : IComparable<T>
    {
        int head = start, tail = end - 1;
        while (head < tail)
        {
            var comparisonResult = s[head].CompareTo(s[head + 1]);

            if (comparisonResult > 0)
            {
                (s[head], s[head + 1]) = (s[head + 1], s[head]);
                head++;
            }
            else
            {
                (s[tail], s[head + 1]) = (s[head + 1], s[tail]);
                tail--;
            }
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