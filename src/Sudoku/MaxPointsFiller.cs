namespace Sudoku;

public static class MaxPointsFiller
{
    public static IEnumerable<(int r, int c, int d)> GetFillingSequence(Field initial, Field solved)
    {
        var current = initial.CloneWith(0, 0, initial.GetCell(0, 0))!;
        while (!current.IsSolved)
        {
            var bestCell = Enumerable.Range(0, Field.Size)
                .SelectMany(r => Enumerable.Range(0, Field.Size).Select(c => (r, c)))
                .Where(cell => !current.IsCellOccupied(cell.r, cell.c))
                .MaxBy(cell =>
                {
                    var scores = new[]
                    {
                        GetRowScore(current, cell),GetColScore(current, cell),GetSquScore(current, cell)
                    };
                    if (scores.Any(s => s == Field.Size - 2))
                        return scores.Sum() + 50;
                    if (scores.Any(s => s == Field.Size - 1))
                        return scores.Sum() + 100;
                    return scores.Sum();
                });

            var digit = solved.GetCell(bestCell.r, bestCell.c);
            yield return (bestCell.r, bestCell.c, digit);
            current = current.CloneWith(bestCell.r, bestCell.c, digit);
        }
    }

    private static int GetRowScore(Field current, (int r, int c) cell) => Enumerable
        .Range(0, Field.Size)
        .Where(col => current.IsCellOccupied(cell.r, col))
        .Sum(_ => 1);

    private static int GetColScore(Field current, (int r, int c) cell) => Enumerable
        .Range(0, Field.Size)
        .Where(row => current.IsCellOccupied(row, cell.c))
        .Sum(_ => 1);

    private static int GetSquScore(Field current, (int r, int c) cell)
    {
        var counter = 0;
        for (int dr = 0; dr < 3; dr++)
        {
            var r = cell.r / 3 * 3 + dr;
            for (int dc = 0; dc < 3; dc++)
            {
                var c = cell.c / 3 * 3 + dc;
                if (current.IsCellOccupied(r, c))
                {
                    counter++;
                }
            }
        }
        return counter;
    }
}