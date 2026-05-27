using System.Buffers;
using System.Numerics.Tensors;

namespace Udemy.Fin.Stat;

public static class AutoCorrelationCalculator
{
    public static double[] GetAutoCorrelationSimd(this double[]? sample, int maxLag)
    {
        //guard clauses
        if (sample is null || sample.Length == 0) return [];
        if (maxLag >= sample.Length)
            throw new InvalidOperationException(
                $"max lag {maxLag} must be smaller than the sample size {sample.Length}");

        var rentedBuffer = ArrayPool<double>.Shared.Rent(sample.Length);
        try
        {
            var centered = rentedBuffer.AsSpan(0, sample.Length);
            var mean = TensorPrimitives.Sum(sample) / sample.Length;
            TensorPrimitives.Subtract(sample, mean, centered);

            //normalization factors cancels out, so we drop it here
            var variance = TensorPrimitives.Dot(centered, centered);
            if (variance == 0) return [];//the variance goes into the denominator => avoid crashes

            var acf = new double[maxLag + 1];
            for (int lag = 0; lag <= maxLag; lag++)
            {
                var lhs = centered[..(sample.Length - lag)];
                var rhs = centered[lag..];
                var covariance = TensorPrimitives.Dot(lhs, rhs);

                acf[lag] = covariance / variance;
            }
            return acf;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rentedBuffer);
        }
    }

    public static double[] GetAutoCorrelationParallel(this double[]? sample, int maxLag)
    {
        //guard clauses
        if (sample is null || sample.Length == 0) return [];
        if (maxLag >= sample.Length)
            throw new InvalidOperationException(
                $"max lag {maxLag} must be smaller than the sample size {sample.Length}");

        var rentedBuffer = ArrayPool<double>.Shared.Rent(sample.Length);
        try
        {
            //a sort of pointer to the array => values are in the array
            var centered = rentedBuffer.AsSpan(0, sample.Length);
            var mean = TensorPrimitives.Sum(sample) / sample.Length;
            TensorPrimitives.Subtract(sample, mean, centered);

            //normalization factors cancels out, so we drop it here
            var variance = TensorPrimitives.Dot(centered, centered);
            if (variance == 0) return [];//the variance goes into the denominator => avoid crashes

            var acf = new double[maxLag + 1];
            
            Parallel.For(0, maxLag + 1,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
                lag =>
                {
                    ReadOnlySpan<double> centeredSpan = rentedBuffer.AsSpan(0, sample.Length);
                    var lhs = centeredSpan[..(sample.Length - lag)];
                    var rhs = centeredSpan[lag..];
                    var covariance = TensorPrimitives.Dot(lhs, rhs);

                    acf[lag] = covariance / variance;
                }
            );

            return acf;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rentedBuffer);
        }
    }

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
            for (int i = 0; i < sample.Count - lag; i++)
            {
                covariance += (sample[i] - mean) * (sample[i + lag] - mean);
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

    public static IEnumerable<double> GetAveragesSeries(this IEnumerable<double> source)
    {
        var countSoFar = 0;
        var totalSoFar = 0.0;

        using var iterator = source.GetEnumerator();
        while (iterator.MoveNext())
        {
            countSoFar++;
            totalSoFar += iterator.Current;
            yield return totalSoFar / countSoFar;
        }
    }
}