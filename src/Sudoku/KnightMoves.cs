namespace Sudoku;

internal class KnightMoves
{
    public static IEnumerable<Position> Traverse(int height, int width, Position initialPosition)
    {
        List<Position> hops = [initialPosition];
        var currentStepNumber = 1;
        var currentPosition = initialPosition;

        var field = new ChessField(height, width);
        field.OccupyCell(currentPosition, currentStepNumber);
        field = field.UpdateOutgoingStepsNumber(ChessFieldExtensions.GetPossibleKnightHops);

        while (field.CanMakeAnyMove(currentPosition, ChessFieldExtensions.GetPossibleKnightHops))
        {
            var nextPosition = GetNextMove(field, currentPosition);
            if (nextPosition is null) break;

            currentPosition = nextPosition.Value;
            field.OccupyCell(currentPosition, ++currentStepNumber);
            field = field.UpdateOutgoingStepsNumber(ChessFieldExtensions.GetPossibleKnightHops);
            hops.Add(currentPosition);
        }

        return hops;
    }

    private static Position? GetNextMove(ChessField cf, Position currentPosition)
    {
        var candidates = ChessFieldExtensions.GetPossibleKnightHops(currentPosition, cf.Height, cf.Width)
            .Where(p => cf.CellStates[p.Row, p.Col] == 0)
            .ToArray();
        
        if (!candidates.Any())
            return null;
        
        return candidates.MinBy(p => cf.OutgoingStepsNumber[p.Row, p.Col]);
    }
}

internal readonly record struct Position(int Row, int Col);

internal record ChessField(int Height, int Width)
{
    public int[,] OutgoingStepsNumber { get; init; } = new int[Height, Width];
    public int[,] CellStates { get; } = new int[Height, Width];//0 - empty, 1<->H*W hop number
}

internal static class ChessFieldExtensions
{
    extension(ChessField cf)
    {
        public ChessField UpdateOutgoingStepsNumber(Func<Position, int, int, IEnumerable<Position>> figurePositions)
        {
            var clone = cf with { OutgoingStepsNumber = new int[cf.Height, cf.Width] };

            for (int r = 0; r < cf.Height; r++)
            {
                for (int c = 0; c < cf.Width; c++)
                {
                    if (cf.CellStates[r, c] != 0)//the cell belongs to the path already
                    {
                        clone.OutgoingStepsNumber[r, c] = -1;
                        continue;
                    }

                    var positions = figurePositions(new(r, c), cf.Height, cf.Width);
                    clone.OutgoingStepsNumber[r, c] = positions.Count(p => cf.CellStates[p.Row, p.Col] == 0);
                }
            }

            return clone;
        }

        public ChessField OccupyCell(Position p, int stepNumber)
        {
            var clone = cf with { };
            clone.CellStates[p.Row, p.Col] = stepNumber;
            return clone;
        }

        public bool CanMakeAnyMove(Position current
            , Func<Position, int, int, IEnumerable<Position>> figurePositions) 
            => figurePositions(current, cf.Height, cf.Width).Any(p => cf.CellStates[p.Row, p.Col] == 0);
    }
    
    public static IEnumerable<Position> GetPossibleKnightHops(Position current, int height, int width) => new Position[]
        {
            new(current.Row + 2, current.Col - 1),
            new(current.Row + 2, current.Col + 1),
            new(current.Row - 2, current.Col - 1),
            new(current.Row - 2, current.Col + 1),
            new(current.Row + 1, current.Col - 2),
            new(current.Row + 1, current.Col + 2),
            new(current.Row - 1, current.Col - 2),
            new(current.Row - 1, current.Col + 2),
        }
        .Where(p => 0 <= p.Col && p.Col < width)
        .Where(p => 0 <= p.Row && p.Row < height);
}