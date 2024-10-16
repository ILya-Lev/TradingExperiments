namespace Sudoku;

public class Field
{
    public const int Size = 9;
    private readonly int[][] _digits;

    public Field(int[][] digits)
    {
        _digits = new int[Size][];
        for (int r = 0; r < Size; r++)
        {
            _digits[r] = new int[Size];
            for (int c = 0; c < Size; c++)
                _digits[r][c] = digits[r][c];
        }
    }

    public Field? CloneWith(int row, int col, int digit)
    {
        if (digit == 0 && IsCellOccupied(row, col))
            return null;

        if (digit != 0 && Contradicts(row, col, digit))
            return null;

        var clone = new Field(_digits);
        clone._digits[row][col] = digit;
        return clone;
    }

    private bool Contradicts(int row, int col, int digit)
    {
        if (_digits[row][col] == digit)
            return false;

        if (_digits[row].Contains(digit)) 
            return true;

        for (int r = 0; r < Size; r++)
        {
            if (_digits[r][col] == digit)
                return true;
        }

        return GetSqrDigits(row, col).Contains(digit);
    }

    public string Print() => string.Join(Environment.NewLine
        , _digits.Select(row => string.Join(", "
            , row.Select(digit => $"{digit}")))
    );

    public int GetCell(int r, int c) => _digits[r][c];
    public bool IsCellOccupied(int r, int c) => _digits[r][c] != 0;

    public bool IsSolved => Enumerable.Range(0, Size).All(r => GetRowMissingDigits(r).Length == 0);

    public int[] GetRowMissingDigits(int row) => Enumerable.Range(1, Size).Except(_digits[row]).ToArray();

    public int[] GetColMissingDigits(int col)
    {
        var presentDigits = new List<int>();
        for (int row = 0; row < Size; row++)
        {
            if (_digits[row][col] != 0)
                presentDigits.Add(_digits[row][col]);
        }

        return Enumerable.Range(1, 9).Except(presentDigits).ToArray();
    }

    public int[] GetSqrMissingDigits(int row, int col)
    {

        var presentDigits = GetSqrDigits(row, col);
        return Enumerable.Range(1, 9).Except(presentDigits).ToArray();
    }

    private IEnumerable<int> GetSqrDigits(int row, int col)
    {
        for (int dr = 0; dr < 3; dr++)
        {
            var r = row / 3 * 3 + dr;
            for (int dc = 0; dc < 3; dc++)
            {
                var c = col / 3 * 3 + dc;
                if (_digits[r][c] != 0)
                    yield return _digits[r][c];
            }
        }
    }
}