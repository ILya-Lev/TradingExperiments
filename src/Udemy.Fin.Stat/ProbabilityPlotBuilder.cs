namespace Udemy.Fin.Stat;

public static class ProbabilityPlotBuilder
{
    public static IEnumerable<(double x, double y)> GetPoints(this IList<double> sample, Func<double, double> invCdf)
    {
        var sortedSample = sample.OrderBy(n => n).ToArray();

        for (int i = 0; i < sortedSample.Length; i++)
        {
            //i+1 as i starts from 0, but is expected to start from 1; -0.5 from the formula
            var percentile = (i + 1 - 0.5) / sortedSample.Length;
            yield return (invCdf(percentile), sortedSample[i]);
        }
    }
}