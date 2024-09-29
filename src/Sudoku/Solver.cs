namespace Sudoku;

public static class Solver
{
    public static Field Solve(Field initial)
    {
        var current = initial;
        //while (!current.IsSolved)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var next = FitByMissingIntersection(current);
                if (object.ReferenceEquals(next, current))
                    break;
                current = next;
            }
        }
        return current;
    }

    private static Field FitByMissingIntersection(Field current)
    {
        var next = current;

        for (int r = 0; r < Field.Size; r++)
        {
            var rowMissingDigits = current.GetRowMissingDigits(r);
            for (int c = 0; c < Field.Size; c++)
            {
                if (current.IsCellOccupied(r, c))
                    continue;

                if (!object.ReferenceEquals(next, current))
                {
                    current = next;
                    rowMissingDigits = current.GetRowMissingDigits(r);
                }

                var colMissingDigits = current.GetColMissingDigits(c);
                var sqrMissingDigits = current.GetSqrMissingDigits(r, c);

                var commonMissingDigits = rowMissingDigits
                    .Intersect(colMissingDigits)
                    .Intersect(sqrMissingDigits)
                    .ToArray();

                if (commonMissingDigits.Length == 1)
                    next = current.CloneWith(r, c, commonMissingDigits[0]);
            }
        }
        
        return next;
    }
}