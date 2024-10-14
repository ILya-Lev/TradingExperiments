using System.Text;

namespace Sudoku;

public static class BinaryGameFrequencyFinder
{
    /// <summary>
    /// there is a game: player one (p1) picks a number T [1; 100)
    /// player two (p2) tries to guess the number: randomly select N
    /// p1 tells either the T is above or below N
    /// 
    /// according to the number of guesses p2 makes until T is found, p1 gives money to p2:
    /// 1 try -> +5; 2 tries -> +4; 3 -> +3; 4 -> +2; 5 -> +1; 6 -> 0; 7 -> -1; 8 -> -2 ....
    /// 
    /// straightforward p2 strategy is to use binary search
    /// thus p1 strategy is to use numbers whos binary search path is 7
    ///
    /// what if p2 understands that p1 is biased towards T with path of 7 steps and shifts the middle points?
    /// </summary>
    /// <param name="first"></param>
    /// <param name="last"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<string> BuildFrequencyReport(int first, int last)
    {
        var paths = Enumerable
            .Range(first, last - first)
            .Select(target => FindTernaryPath(first, last, target).Distinct().ToList())
            .ToArray();

        var pathFrequency = paths
            .GroupBy(p => p.Count, p => p)
            .OrderByDescending(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        return paths
            .Select(path => new StringBuilder()
                .Append($"number: {path.Last()}; steps: {path.Count}; path: ")
                .AppendJoin("->", path)
                .ToString())
            .Concat(pathFrequency.Select(p => $"Path of length {p.Key} is met {p.Value} times."))
            .ToArray();
    }

    private static IReadOnlyCollection<int> FindBinaryPath(int first, int last, int target)
    {
        if (target < first || last < target || first > last)
            return [];

        var start = first;
        var end = last;
        var middle = (first + last) / 2;

        var path = new List<int>() { middle };
        while (middle != target)
        {
            if (target < middle)
                end = middle;
            else
                start = middle;

            middle = (start + end) / 2;
            path.Add(middle);
        }

        return path;
    }

    private static IReadOnlyCollection<int> FindSkewedPath(int first, int last, int target)
    {
        if (target < first || last < target || first > last)
            return [];

        var start = first;
        var end = last;
        var middle = (first + last) / 2;

        var path = new List<int>() { middle };
        while (middle != target)
        {
            if (target < middle)
                end = middle - 1;
            else
                start = middle + 1;

            middle = (start + end) / 2;
            path.Add(middle);
        }

        return path;
    }

    private static IReadOnlyCollection<int> FindTernaryPath(int first, int last, int target)
    {
        if (target < first || last < target || first > last)
            return [];

        var start = first;
        var end = last;
        var width = (last - first) / 3;
        var middle = first + width;

        var path = new List<int>();
        while (true)
        {
            path.Add(middle);
            if (target == middle) return path;

            if (target < middle)
            {
                end = middle - 1;
            }
            else
            {
                var m2 = middle + width;
                path.Add(m2);
                if (target == m2) return path;

                if (target < m2)
                {
                    start = middle + 1;
                    end = m2 - 1;
                }
                else
                    start = m2 + 1;
            }

            width = (end - start) / 3;
            middle = start + width;
        }
    }
}