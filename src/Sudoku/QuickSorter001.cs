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

            int head = start, tail = end - 1;
            while(head < tail)
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

            if (!HasConverged(s, start, head)) boundaries.Push((start, head));
            if (!HasConverged(s, head, end)) boundaries.Push((head, end));
        }

        return s;
    }

    private static bool HasConverged<T>(IReadOnlyList<T> s, int start, int end)
        where T: IComparable<T>
    {
        for (var i = start; i+1 < end; i++)
        {
            var comparisonResult = s[i].CompareTo(s[i + 1]);
            if (comparisonResult > 0) return false;
        }

        return true;
    }

    //todo: make it parallel via task-based parallelism
    public static IList<T> SortParallel<T>(IList<T> source) where T : IComparable<T>
    {
        var boundaries = new Stack<(int start, int end)>();
        boundaries.Push((0, source.Count));

        while (boundaries.Any())
        {
            var (start, end) = boundaries.Pop();
            var seedIndex = Random.Shared.Next(start, end);
            (source[start], source[seedIndex]) = (source[seedIndex], source[start]);//swap

            int head = start, tail = end - 1;
            while(head < tail)
            {
                var comparisonResult = source[head].CompareTo(source[head + 1]);

                if (comparisonResult > 0)
                {
                    (source[head], source[head + 1]) = (source[head + 1], source[head]);
                    head++;
                }
                else
                {
                    (source[tail], source[head + 1]) = (source[head + 1], source[tail]);
                    tail--;
                }
            }

            if (start < head - 1) boundaries.Push((start, head));
            if (head < end - 1) boundaries.Push((head, end));
        }

        return source;
    }
}