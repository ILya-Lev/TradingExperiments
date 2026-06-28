using System.Buffers;
using System.Numerics;
using System.Numerics.Tensors;
using MathNet.Numerics.Distributions;

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

    public static double[] GetAutoCorrelationParallel(this double[] sample, int maxLag) 
        => sample.AsSpan().GetAutoCorrelationParallel(maxLag);

    public static double[] GetAutoCorrelationParallel(this Span<double> sample, int maxLag)
    {
        //guard clauses
        var n = sample.Length;
        if (n == 0) return [];
        if (maxLag >= sample.Length)
            throw new InvalidOperationException($"max lag {maxLag} must be smaller than the sample size {n}");

        var rentedBuffer = ArrayPool<double>.Shared.Rent(n);
        try
        {
            //a sort of pointer to the array => values are in the array
            var centered = rentedBuffer.AsSpan(0, n);
            var mean = TensorPrimitives.Sum(sample) / n;
            TensorPrimitives.Subtract(sample, mean, centered);

            //normalization factors cancels out, so we drop it here
            var variance = TensorPrimitives.Dot(centered, centered);
            if (variance == 0) return [];//the variance goes into the denominator => avoid crashes

            var acf = new double[maxLag + 1];
            
            Parallel.For(0, maxLag + 1,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
                lag =>
                {
                    ReadOnlySpan<double> centeredSpan = rentedBuffer.AsSpan(0, n);
                    var lhs = centeredSpan[..(n - lag)];
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

    public static (decimal qStat, decimal pValue) GetLjungBoxTestValues(this ReadOnlySpan<decimal> sample, int maxLag)
    {
        var n = sample.Length;
        
        var mean = TensorPrimitives.Average(sample);
        var centered = new Span<decimal>(new decimal[n]);
        TensorPrimitives.Subtract(sample, mean, centered);

        var varianceSum = TensorPrimitives.Dot(centered, centered);
        var qStat = 0m;
        for (var lag = 1; lag <= maxLag; lag++)
        {
            var lhs = centered[lag..];
            var rhs = centered[..(n - lag)];
            var covarSum = TensorPrimitives.Dot(lhs, rhs);
            var rhoLag = covarSum / varianceSum;

            qStat += rhoLag*rhoLag/(n-lag);
        }
        qStat *= n * (n + 2);

        var pValue = 1m - (decimal)ChiSquared.CDF(maxLag, (double)qStat);
        return (qStat, pValue);
    }

    public static (decimal qStat, decimal pValue) GetLjungBoxTestValuesFromAcf(this IReadOnlyCollection<decimal> acfs, int sampleSize)
    {
        var n = sampleSize;
        var qStat = 0m;
        var lag = 1;
        foreach(var acf in acfs.Skip(1))
        {
            qStat += acf*acf/(n-lag);
            lag++;
        }

        qStat *= n * (n + 2);

        var pValue = 1m - (decimal)ChiSquared.CDF(acfs.Count-1, (double)qStat);
        return (qStat, pValue);
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

    public static double CorrelationFunction(this double[] lhs, double[] rhs)
    {
        if (lhs.Length != rhs.Length)
            throw new ArgumentException($"lhs length {lhs.Length} mismatch rhs length {rhs.Length}");

        if (lhs.Length <= 1)
            throw new InvalidOperationException("Too small sample data, needs at least 2 items");

        var n = lhs.Length;
        var lhsMean = TensorPrimitives.Sum(lhs) / n;
        var rhsMean = TensorPrimitives.Sum(rhs) / n;
        
        var centeredLhs = new Span<double>(new double[n]);
        TensorPrimitives.Subtract(lhs, lhsMean, centeredLhs);
        
        var centeredRhs = new Span<double>(new double[n]);
        TensorPrimitives.Subtract(rhs, rhsMean, centeredRhs);

        var varLhs = TensorPrimitives.SumOfSquares(centeredLhs) / (n - 1);
        var varRhs = TensorPrimitives.SumOfSquares(centeredRhs) / (n - 1);

        if (varLhs == 0 || varRhs == 0)
            throw new InvalidOperationException($"variance of one of the series is zero: lhs {varLhs} and rhs {varRhs}");

        var covariance = TensorPrimitives.Dot(centeredLhs, centeredRhs) / (n - 1);
        var correlation = covariance / Math.Sqrt(varLhs * varRhs);
        return correlation;
    }
}