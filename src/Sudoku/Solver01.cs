using System.Diagnostics;

namespace Sudoku01;

[DebuggerDisplay("{Row} {Column} {Digit}")]
public class Cell
{
    private readonly List<int> _possibleDigits;

    public Cell(int row, int column, int digit = 0)
    {
        Row = row;
        Column = column;
        Digit = digit;

        _possibleDigits = digit == 0
            ? Enumerable.Range(1, 9).ToList()
            : [];
    }

    public bool IsEmpty => Digit == 0;
    public IReadOnlyList<int> PossibleDigits => _possibleDigits;
    public int Row { get; }
    public int Column { get; }
    public int Digit { get; private set; }

    public void Deconstruct(out int row, out int column, out int digit)
    {
        row = Row;
        column = Column;
        digit = Digit;
    }

    public void RemovePossibleDigits(params int[] digits)
    {
        foreach (var d in digits)
            RemovePossibleDigit(d);
    }

    public void RemovePossibleDigit(int digit)
    {
        if (!IsEmpty) return;

        if (digit is < 1 or > 9)
            throw new ArgumentOutOfRangeException(nameof(digit), $"Possible digit must be between 1 and 9, provided: {digit}");

        var indexOfDigit = _possibleDigits.IndexOf(digit);
        if (indexOfDigit >= 0)
            _possibleDigits.RemoveAt(indexOfDigit);

        if (_possibleDigits.Count == 0)
            throw new InvalidOperationException($"After removing {digit}, no possible digits remain for cell ({Row}, {Column}).");
    }

    public void SetDigit(int digit)
    {
        if (!IsEmpty)
            throw new InvalidOperationException($"Cell ({Row}, {Column}) already has a digit assigned: {Digit}. Attempt to set {digit} to it fails.");

        if (digit is < 1 or > 9)
            throw new ArgumentOutOfRangeException(nameof(digit), $"Digit must be between 1 and 9, provided: {digit}");

        Digit = digit;
        _possibleDigits.Clear();
    }

    public bool IsTheSame(Cell other) => other.Row == Row && other.Column == Column;

    public void SetPossible(params int[] possibleDigits)
    {
        if (!IsEmpty)
            throw new InvalidOperationException($"Cell ({Row}, {Column}) already has a digit assigned: {Digit}. Attempt to set possible digits to it fails.");
        if (possibleDigits.Length == 0)
            throw new InvalidOperationException($"Cannot set possible digits set to an empty array within method {nameof(SetPossible)}");
        if (possibleDigits.Any(d => d is < 1 or > 9))
            throw new ArgumentOutOfRangeException(nameof(possibleDigits), $"All possible digits must be between 1 and 9, provided: {string.Join(", ", possibleDigits)}");

        if (possibleDigits.Length == 1)
        {
            SetDigit(possibleDigits[0]);
            return;
        }

        _possibleDigits.Clear();
        _possibleDigits.AddRange(possibleDigits.Distinct().Order());
    }
}

public class Field(Cell[] cells)
{
    public const int Size = 9;
    public IReadOnlyList<Cell> Cells { get; } = cells;
    public bool IsSolved => Cells.All(c => !c.IsEmpty);
    public int UnsolvedCount => Cells.Count(c => c.IsEmpty);
    public Cell GetCell(int row, int column) => Cells[row * Size + column];

    public IReadOnlyList<Cell> GetRow(int row) => Cells.Where(c => c.Row == row).ToArray();

    public IReadOnlyList<Cell> GetColumn(int column) => Cells.Where(c => c.Column == column).ToArray();

    public IReadOnlyList<Cell> GetSquare(int square) => Cells.Where(c => c.Row / 3 * 3 + c.Column / 3 == square).ToArray();

    public IReadOnlyList<Cell> GetSquare(int row, int column) => GetSquare(row / 3 * 3 + column / 3);

    public IEnumerable<string> FindInconsistencies()
    {
        for (int r = 0; r < Size; r++)
            if (ContainsDuplicate(GetRow(r)))
                yield return $"There is inconsistency in row {r}";

        for (int c = 0; c < Size; c++)
            if (ContainsDuplicate(GetColumn(c)))
                yield return $"There is inconsistency in column {c}";

        for (int s = 0; s < Size; s++)
            if (ContainsDuplicate(GetSquare(s)))
                yield return $"There is inconsistency in square {s}";
    }

    private static bool ContainsDuplicate(IReadOnlyList<Cell> structure)
    {
        var digits = structure.Where(c => !c.IsEmpty).Select(c => c.Digit).ToArray();
        var uniqueDigits = digits.Distinct().ToArray();
        return digits.Length != uniqueDigits.Length;
    }
}

public static class Solver01
{
    public static Field Solve(Field initial)
    {
        var current = new Field(initial.Cells.Select(c => new Cell(c.Row, c.Column, c.Digit)).ToArray());

        for (int attempt = 0; attempt < 20; attempt++)
        {
            var before = current.UnsolvedCount;
            foreach (var cell in current.Cells)
            {
                if (!cell.IsEmpty)
                    continue;

                TryFillStructure(current, current.GetRow(cell.Row));
                TryFillStructure(current, current.GetColumn(cell.Column));
                TryFillStructure(current, current.GetSquare(cell.Row, cell.Column));
            }

            var after = current.UnsolvedCount;
            if (after == 0) return current;//i.e. already solved

            if (after < before) continue;   //i.e., this mechanism still works

            foreach (var cell in current.Cells)
            {
                if (!cell.IsEmpty)
                    continue;

                RestrictPossibleDigitsByPair(current, current.GetRow(cell.Row));
                RestrictPossibleDigitsByPair(current, current.GetColumn(cell.Column));
                RestrictPossibleDigitsByPair(current, current.GetSquare(cell.Row, cell.Column));
            }
        }

        return current;
    }

    private static void TryFillStructure(Field field, IReadOnlyList<Cell> structure)
    {
        var occupiedDigits = structure.Where(c => !c.IsEmpty).Select(c => c.Digit).ToArray();
        foreach (var cell in structure.Where(c => c.IsEmpty))
        {
            cell.RemovePossibleDigits(occupiedDigits);
            if (cell.PossibleDigits.Count == 1)
                SetDigit(cell, cell.PossibleDigits[0], field);
        }
    }

    private static void SetDigit(Cell cell, int digit, Field field)
    {
        cell.SetDigit(digit);

        RemovePossibleDigitOnSetDigit(field.GetRow(cell.Row), digit);
        RemovePossibleDigitOnSetDigit(field.GetColumn(cell.Column), digit);
        RemovePossibleDigitOnSetDigit(field.GetSquare(cell.Row, cell.Column), digit);
    }

    private static void RemovePossibleDigitOnSetDigit(IReadOnlyList<Cell> structure, int digit)
    {
        foreach (var sibling in structure.Where(c => c.IsEmpty))
            sibling.RemovePossibleDigit(digit);
    }

    private static void RestrictPossibleDigitsByPair(Field field, IReadOnlyList<Cell> structure)
    {
        var dwc = GroupCellsByPossibleDigits(structure);

        for (int k = 0; k < dwc.Length; k++)
        {
            if (dwc[k].Cells.Length == 1) // && dwc[k].Cells[0].IsEmpty)
            {
                SetDigit(dwc[k].Cells[0], dwc[k].Digit, field);
            }
        }

        dwc = GroupCellsByPossibleDigits(structure);
        for (int i = 0; i < dwc.Length; i++)
        {
            if (dwc[i].Cells.Length != 2) continue;
            var currentFirst = dwc[i].Cells[0];
            var currentSecond = dwc[i].Cells[1];

            for (int j = i + 1; j < dwc.Length; j++)
            {
                if (dwc[j].Cells.Length != 2) continue;
                var otherFirst = dwc[j].Cells[0];
                var otherSecond = dwc[j].Cells[1];

                if ((currentFirst.IsTheSame(otherFirst) && currentSecond.IsTheSame(otherSecond))
                 || (currentFirst.IsTheSame(otherSecond) && currentSecond.IsTheSame(otherFirst)))
                {
                    var possibleDigits = new[] { dwc[i].Digit, dwc[j].Digit };
                    currentFirst.SetPossible(possibleDigits);
                    currentSecond.SetPossible(possibleDigits);

                    return;
                }
            }
        }
    }

    private static (int Digit, Cell[] Cells)[] GroupCellsByPossibleDigits(IReadOnlyList<Cell> s) => s
        .Where(c => c.IsEmpty)
        .SelectMany(c => c.PossibleDigits.Select(d => (d, c)))
        .GroupBy(dc => dc.d, dc => dc.c)
        .Select(g => (g.Key, g.ToArray()))
        .ToArray();
}