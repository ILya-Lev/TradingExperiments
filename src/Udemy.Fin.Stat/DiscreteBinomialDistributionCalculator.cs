namespace Udemy.Fin.Stat;

public static class DiscreteBinomialDistributionCalculator
{
    /// <summary> cdf stands for cumulative distribution function </summary>
    public static IReadOnlyDictionary<double, double> ConvertDistributionIntoCdf(
        this IReadOnlyDictionary<double, double> distribution)
    {
        var total = 0.0;
        return distribution.ToDictionary(p => p.Key, p => total += p.Value);
    }

    public static double GetUpToProbability(int sampleSize, int occurrences, double success = 0.5) =>
        Enumerable.Range(0, occurrences+1)
            .Sum(oc => GetOccurrencesProbability(sampleSize, oc, success));
    
    public static double GetOccurrencesProbability(int sampleSize, int occurrences, double success = 0.5) =>
        sampleSize.GetCombinationsNumber(occurrences)
        * Math.Pow(success, occurrences)
        * Math.Pow(1 - success, sampleSize - occurrences);

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