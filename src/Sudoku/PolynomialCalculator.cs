namespace Sudoku;

public static class PolynomialCalculator
{
    /// <summary>
    /// Calculates the value of a polynomial at a given point using Horner's method.
    /// a_0 + a_1 * x + a_2 * x^2 + ... + a_n * x^n
    /// i.e. a_0 is not multiplied by the value of x.
    /// </summary>
    /// <param name="coefficients">the first element (a_0) is not multiplied by x; number of coefficients define the order of the polynomial</param>
    /// <param name="x">the point where the value of the polynomial is to be calculated</param>
    /// <returns>value of the polynomial in a given point (x)</returns>
    public static decimal GetValue(this IReadOnlyList<decimal> coefficients, decimal x)
    {
        var value = 0m;
        for (int i = coefficients.Count - 1; i > 0; i--)
        {
            value += coefficients[i];
            value *= x;
        }
        return value + coefficients[0];
    }

    public static decimal GetValueLinq(this IReadOnlyList<decimal> coefficients, decimal x)
        => coefficients
            .Select((a, i) => (a, i))
            .Sum(item => item.a * (decimal)Math.Pow((double)x, item.i));

    public static decimal GetValueStraightforward(this IReadOnlyList<decimal> coefficients, decimal x)
    {
        var value = coefficients[0];
        for (int i = 1; i < coefficients.Count; i++)
        {
            value += coefficients[i] * (decimal)Math.Pow((double)x, i);
        }
        return value;
    }

    public static decimal GetValuePlinq(this IReadOnlyList<decimal> coefficients, decimal x)
        => coefficients
            .Select((a, i) => (a, i))
            .AsParallel()
            .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount-1))
            .Aggregate(
                seedFactory: () => 0m,
                updateAccumulatorFunc: (acc, item) => item.a * (decimal)Math.Pow((double)x, item.i),
                combineAccumulatorsFunc: (lhs, rhs) => lhs+rhs,
                resultSelector: acc => acc);

}