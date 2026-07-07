using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using MathNet.Numerics.Distributions;

namespace Udemy.Fin.Stat;

/// <summary>
/// upper and lower confidence bounds when the underlying distribution is normal
/// and standard deviation is known
/// </summary>
public static class NormalConfidenceBounds
{
    public static (double lower, double upper) GetMuInterval(this double mean, int count, double confidenceLevel,
        double stdDev = 1.0)
        => GetMuBounds(mean, count, 1 - (1 - confidenceLevel) / 2, stdDev);

    public static (double lower, double upper) GetMuBounds(this double mean, int count, double confidenceLevel, double stdDev = 1.0)
    {
        var halfRange = stdDev / Math.Sqrt(count) * Normal.InvCDF(0, 1, confidenceLevel);
        return (mean - halfRange, mean + halfRange);
    }

    public static (double lower, double upper) GetMuInterval(this IEnumerable<double> sample, double confidenceLevel, double stdDev = 1.0)
        => GetMuBounds(sample, 1 - (1 - confidenceLevel) / 2, stdDev);

    public static (double lower, double upper) GetMuBounds(this IEnumerable<double> sample, double confidenceLevel, double stdDev = 1.0)
    {
        var (mean, count) = GetAverageAndCount(sample);
        var halfRange = stdDev / Math.Sqrt(count) * Normal.InvCDF(0, 1, confidenceLevel);
        return (mean - halfRange, mean + halfRange);
    }

    public static (double z, double p) GetPValue(this IEnumerable<double> sample, double mu = 0.0, double stdDev = 1.0)
    {
        var (mean, count) = GetAverageAndCount(sample);
        var z = (mean - mu) / stdDev * Math.Sqrt(count);
        var p = Normal.CDF(0, 1, z);
        return (z, p);
    }

    public static double GetCriticalRegion(this double significanceLevel) => Normal.InvCDF(0, 1, 1-significanceLevel);

    private static (double mean, int count) GetAverageAndCount(IEnumerable<double> sample) => sample switch
    {
        double[] array => (TensorPrimitives.Average(array), array.Length),
        List<double> list => (TensorPrimitives.Average(CollectionsMarshal.AsSpan(list)), list.Count),
        _ => GetAverageAndCountNaive(sample)
    };

    private static (double mean, int count) GetAverageAndCountNaive(IEnumerable<double> sample)
    {
        var total = 0.0;
        var count = 0;
        foreach (var number in sample)
        {
            total += number;
            count++;
        }

        var mean = total / count;
        return (mean, count);
    }
}
