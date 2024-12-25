using System.Collections.Frozen;

namespace Sudoku;

public class HanoiTowers
{
    public static string Materialize((int current, int from, int to) move) => $"{move.current}: {move.from} -> {move.to}";
    public static IEnumerable<(int current, int from, int to)> Solve(int n) => DoSolve(n, 1, 2, 3);

    //from concat to a couple of forech = from exponential memory consumption growth to constant ~21KB vs 12GB+
    //from second recursive call to while(true) = 4x time reduction for n=20
    private static IEnumerable<(int current, int from, int to)> DoSolve(int n, int start, int buffer, int end)
    {
        while (true)
        {
            if (n > 1)
            {
                foreach (var before in DoSolve(n - 1, start, end, buffer)) yield return before;
            }

            yield return (n, start, end);
            if (n == 1)
            {
                yield break;
            }

            if (n > 1)
            {
                var start1 = start;
                n = n - 1;
                start = buffer;
                buffer = start1;
                continue;
            }

            break;
        }
    }

    //an attempt to save time on generating actual path
    private static readonly FrozenDictionary<(int, int), string> _steps = new Dictionary<(int, int), string>()
    {
        [(1, 2)] = "1 -> 2",
        [(1, 3)] = "1 -> 3",
        [(2, 1)] = "2 -> 1",
        [(2, 3)] = "2 -> 3",
        [(3, 1)] = "3 -> 1",
        [(3, 2)] = "3 -> 2",
    }.ToFrozenDictionary();

}