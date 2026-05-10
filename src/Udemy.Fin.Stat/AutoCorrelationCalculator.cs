namespace Udemy.Fin.Stat;

public static class AutoCorrelationCalculator
{
    public static double[] GetAutoCorrelation(this IReadOnlyList<double>? sample, int maxLag)
    {
        //guard clauses
        if (sample is null || sample.Count == 0) return [];
        if (maxLag >= sample.Count)
            throw new InvalidOperationException(
                $"max lag {maxLag} must be smaller than the sample size {sample.Count}");

        var acf = new double[maxLag + 1];

        var mean = sample.Average();
        //normalization factors cancels out, so we drop it here
        var variance = sample.GetSampleVariance(mean) * (sample.Count - 1);

        if (variance == 0) return [];//the variance goes into the denominator => avoid crashes

        for (int lag = 0; lag < maxLag; lag++)
        {
            var covariance = 0.0;
            for (int i = 0; i < sample.Count-lag; i++)
            {
                covariance += (sample[i] - mean)*(sample[i+lag]-mean);
            }

            acf[lag] = covariance / variance;
        }

        return acf;
    }

    public static double GetSampleVariance(this IReadOnlyList<double> sample, double mean)
    {
        var nominator = 0.0;
        foreach (var x in sample)
        {
            nominator += (x - mean) * (x - mean);
        }

        return nominator / (sample.Count - 1);
    }
}