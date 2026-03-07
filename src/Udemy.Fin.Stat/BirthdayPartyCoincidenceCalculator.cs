namespace Udemy.Fin.Stat;

public static class BirthdayPartyCoincidenceCalculator
{
    //O(n^2) - nested loop to get the product
    public static IEnumerable<double> Generate2OrMoreSameDaySeries(int n)
        => Enumerable.Range(1, n).Select(Calculate2OrMoreSameDay);

    public static double Calculate2OrMoreSameDay(int n)
        => 1.0 - CalculateDifferentBirthdaysProbability(n);

    public static double CalculateDifferentBirthdaysProbability(int n)
        => Enumerable.Range(0, n)
            .Select(i => (365 - i) / 365.0)
            .Aggregate(1.0, (acc, c) => acc * c);

    //O(n) - no nested loops
    public static IEnumerable<double> Generate2OrMoreSameDaySeriesFast(int n)
    {
        var occupations = Enumerable.Range(0, n).Select(i => (365 - i) / 365.0).ToArray();
        var pAllDifferent = 1.0;
        foreach (var pOneDifferent in occupations)
        {
            pAllDifferent *= pOneDifferent;
            yield return 1 - pAllDifferent;
        }
    }
}

