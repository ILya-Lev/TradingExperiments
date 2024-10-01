namespace Sudoku;

public static class Solver
{
    public static Field? Solve(Field? initial)
    {
        if (initial == null) return null;

        var states = new Stack<Field>();
        states.Push(initial);

        while (states.Any())
        {
            var current = states.Pop();

            var next = PopulateByOneIntersection(current);
            if (next is null)
                continue;
            
            current = next;
            if (current.IsSolved)
                return current;

            foreach (var childState in DoubleSplitFit(current).Where(s => s is not null))
                states.Push(childState!);
        }
        
        return null;
    }

    private static Field? PopulateByOneIntersection(Field current)
    {
        var next = current;
        do
        {
            current = next;
            next = FitByMissingIntersection(current);
            if (next == null)
                return null;


        } while (!object.ReferenceEquals(next, current));

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

    private static Field?[] DoubleSplitFit(Field current)
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
                    return new[]
                    {
                        current.CloneWith(r, c, commonMissingDigits[0]),
                        current.CloneWith(r, c, commonMissingDigits[1])
                    };
                }
            }
        }
        
        return Array.Empty<Field?>();
    }

    private static int[] FindCommonMissingDigits(Field current, int r, int c) => current
        .GetRowMissingDigits(r)
        .Intersect(current.GetColMissingDigits(c))
        .Intersect(current.GetSqrMissingDigits(r, c))
        .ToArray();
}