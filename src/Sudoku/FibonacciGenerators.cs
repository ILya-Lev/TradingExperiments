namespace Sudoku;

public static class FibonacciGenerators
{
    public static IEnumerable<int> FibonacciSequence()
    {
        int a = 1, b = 1;
        yield return a;
        yield return b;
        while (true)
        {
            var tmp = a + b;
            a = b;
            b = tmp;
            yield return tmp;
        }
    }

    /// <summary>
    /// are used for calculating: key retracement levels
    /// if price is 10$ now, it may fall by fibonacci ratio, e.g., by 38.2% => 6.18$
    /// </summary>
    public static IEnumerable<double> FibonacciRatios(int counter, int initialShift = 10)
    {
        using var iterator = FibonacciSequence().GetEnumerator();

        //skip first 10
        for(int i = 0; i < initialShift; i++)
            iterator.MoveNext();
        
        var current = iterator.Current;

        for (int i = 0; i < counter; i++, iterator.MoveNext())
            yield return current * 1.0 / iterator.Current;
    }
}