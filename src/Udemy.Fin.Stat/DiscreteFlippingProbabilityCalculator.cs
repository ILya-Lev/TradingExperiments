namespace Udemy.Fin.Stat;

public static class DiscreteFlippingProbabilityCalculator
{
    /// <summary> cdf stands for cumulative distribution function </summary>
    public static IReadOnlyDictionary<double, double> ConvertDistributionIntoCdf(
        this IReadOnlyDictionary<double, double> distribution)
    {
        var total = 0.0;
        return distribution.ToDictionary(p => p.Key, p => total += p.Value);
    }

    public static double GetOccurrencesProbability(int flips, int occurrences, double headsProbability = 0.5) =>
        flips.GetCombinationsNumber(occurrences)
        * Math.Pow(headsProbability, occurrences)
        * Math.Pow(1 - headsProbability, flips - occurrences);

    /// <summary>
    /// aka binomial coefficient = n!/k!/(n-k)!
    /// (n)
    /// (k)
    /// </summary>
    /// <param name="n"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static double GetCombinationsNumber(this int n, int k)
    {
        var result = 1.0;
        var min = k < (n - k) ? k : (n - k);
        for (int i = 0; i < min; i++)
        {
            result *= 1.0 * (n - i) / (min - i);
        }
        return result;
    }
}