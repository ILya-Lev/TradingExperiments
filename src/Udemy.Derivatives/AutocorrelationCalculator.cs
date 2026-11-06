using System.Collections.Concurrent;
using System.Numerics.Tensors;

namespace Udemy.Derivatives;

public static class AutocorrelationCalculator
{
    public static IReadOnlyList<double> GetAutocorrelationPerLag(this double[] sample, int? maxLag = null)
    {
        var autocorrelations = new double[maxLag ?? sample.Length];
        autocorrelations[0] = 1;

        var mean = TensorPrimitives.Average(sample);
        var centered = new double[sample.Length];
        TensorPrimitives.Subtract(sample, mean, centered);
        var variance = TensorPrimitives.Dot(centered, centered);//not exactly variance as we omit 1/N

        //for (var lag = 1; lag < (maxLag ?? sample.Length); ++lag)
        //{
        //    //not exactly autocovariance as we omit 1/N
        //    var autocovariance = TensorPrimitives.Dot(centered[..(sample.Length - lag)], centered[lag..]);

        //    // we omit 1/N as here it vanishes => remove redundant division N times
        //    autocorrelations[lag] = autocovariance / variance;
        //}

        Parallel.For(1, (maxLag ?? sample.Length),
            new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1}, lag =>
            {
                //a reference to trick compiler around and use ranges for slicing
                ReadOnlySpan<double> span = centered;

                //not exactly autocovariance as we omit 1/N
                var autocovariance = TensorPrimitives.Dot(span[..(sample.Length - lag)], span[lag..]);

                // we omit 1/N as here it vanishes => remove redundant division N times
                autocorrelations[lag] = autocovariance / variance;
            });


        return autocorrelations;
    }
}