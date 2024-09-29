namespace Sudoku;

public static class Solver
{
    public static Field? Solve(Field? initial)
    {
        if (initial == null) return null;

        var current = initial;
        while (!current.IsSolved)
        {
            //for (int attempt = 0; attempt < 10; attempt++)
            while (true)
            {
                var next = FitByMissingIntersection(current);
                if (next == null) 
                    return null;

                if (object.ReferenceEquals(next, current))
                    break;

                current = next;
            }

            if (current.IsSolved)
                return current;

            current = DoubleSplitFit(current) ?? current;
        }
        return current;
    }

    private static Field? FitByMissingIntersection(Field current)
    {
        for (int r = 0; r < Field.Size; r++)
        {
            for (int c = 0; c < Field.Size; c++)
            {
                if (current.IsCellOccupied(r, c))
                    continue;

                var commonMissingDigits = FindCommonMissingDigits(current, r, c);

                if (commonMissingDigits.Length == 0)
                    return null;

                if (commonMissingDigits.Length == 1)
                    current = current.CloneWith(r, c, commonMissingDigits[0])!;
            }
        }

        return current;
    }

    private static Field? DoubleSplitFit(Field current)
    {
        for (int r = 0; r < Field.Size; r++)
        {
            for (int c = 0; c < Field.Size; c++)
            {
                if (current.IsCellOccupied(r, c))
                    continue;

                var commonMissingDigits = FindCommonMissingDigits(current, r, c);

                if (commonMissingDigits.Length == 2)
                {
                    var left = current.CloneWith(r, c, commonMissingDigits[0]);
                    left = Solve(left);
                    if (left?.IsSolved == true)
                        return left;

                    var right = current.CloneWith(r, c, commonMissingDigits[1]);
                    right = Solve(right);
                    return right;
                }
            }
        }
        
        return null;
    }

    private static int[] FindCommonMissingDigits(Field current, int r, int c) => current
        .GetRowMissingDigits(r)
        .Intersect(current.GetColMissingDigits(c))
        .Intersect(current.GetSqrMissingDigits(r, c))
        .ToArray();
}