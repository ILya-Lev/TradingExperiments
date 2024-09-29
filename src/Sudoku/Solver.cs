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

            var doubleFit = DoubleSplitFit(current);
            if (doubleFit is not null) 
                current = doubleFit;
        }
        return current;
    }

    private static Field? FitByMissingIntersection(Field current)
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

                if (commonMissingDigits.Length == 0)
                    return null;

                if (commonMissingDigits.Length == 1)
                    next = current.CloneWith(r, c, commonMissingDigits[0])!;
            }
        }
        
        return next;
    }

    private static Field? DoubleSplitFit(Field current)
    {
        for (int r = 0; r < Field.Size; r++)
        {
            var rowMissingDigits = current.GetRowMissingDigits(r);
            for (int c = 0; c < Field.Size; c++)
            {
                if (current.IsCellOccupied(r, c))
                    continue;

                var colMissingDigits = current.GetColMissingDigits(c);
                var sqrMissingDigits = current.GetSqrMissingDigits(r, c);

                var commonMissingDigits = rowMissingDigits
                    .Intersect(colMissingDigits)
                    .Intersect(sqrMissingDigits)
                    .ToArray();

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
}