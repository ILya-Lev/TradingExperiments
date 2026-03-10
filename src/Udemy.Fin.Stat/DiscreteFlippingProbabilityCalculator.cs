namespace Udemy.Fin.Stat;

public static class DiscreteFlippingProbabilityCalculator
{
    public static double GetOccurrencesProbability(int flips, int occurrences, double headsProbability = 0.5) =>
        flips.GetCombinationsNumber(occurrences)
        * Math.Pow(headsProbability, occurrences)
        * Math.Pow(1 - headsProbability, flips - occurrences);

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