using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using System.Numerics.Tensors;

namespace Udemy.Derivatives;

public static class AutocorrelationCalculator
{
    public static IReadOnlyList<double> GetAutocorrelationPerLag(this double[] sample, int? maxLag = null)
    {
        var lags = maxLag ?? sample.Length;
        if (maxLag == 0 || sample.Length == 1)
            return [1];

        if (lags < 4 * Math.Log2(sample.Length))
            return GetAutocorrelationPerLagNaive(sample, lags);

        return GetAutocorrelationPerLagFft(sample);
    }

    private static IReadOnlyList<double> GetAutocorrelationPerLagNaive(double[] sample, int maxLag)
    {
        var autocorrelations = new double[maxLag];
        autocorrelations[0] = 1;

        var mean = TensorPrimitives.Average(sample);
        var centered = new double[sample.Length];
        TensorPrimitives.Subtract(sample, mean, centered);
        var variance = TensorPrimitives.Dot(centered, centered);//not exactly variance as we omit 1/N

        //for (var lag = 1; lag < maxLag; ++lag)
        //{
        //    //not exactly autocovariance as we omit 1/N
        //    var autocovariance = TensorPrimitives.Dot(centered[..(sample.Length - lag)], centered[lag..]);

        //    // we omit 1/N as here it vanishes => remove redundant division N times
        //    autocorrelations[lag] = autocovariance / variance;
        //}

        Parallel.For(1, maxLag,
            new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, lag =>
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

    private static IReadOnlyList<double> GetAutocorrelationPerLagFft(double[] sample)
    {
        var autocorrelations = new double[sample.Length];
        autocorrelations[0] = 1.0;

        //Control.TryUseNativeMKL();
        Control.UseNativeMKL();

        // Next power of 2 >= 2 * N to prevent circular convolution wrap-around
        var m = (int)BitOperations.RoundUpToPowerOf2((uint)(2 * sample.Length));

        // MathNet's Fourier.Forward(Complex[]) uses the full array.Length => it must be exactly M.
        var spectrum = new Complex[m];

        // 1. Center the signal into the first N elements; [N..M) remain Complex.Zero
        var mean = TensorPrimitives.Average(sample);
        for (var i = 0; i < sample.Length; i++)
        {
            spectrum[i] = new Complex(sample[i] - mean, 0.0);
        }

        // 2. In-place Forward FFT without scaling
        Fourier.Forward(spectrum, FourierOptions.NoScaling);

        // 3. Power spectral density: |C[k]|^2 = Re^2 + Im^2
        for (var i = 0; i < m; i++)
        {
            var re = spectrum[i].Real;
            var im = spectrum[i].Imaginary;
            spectrum[i] = new Complex((re * re) + (im * im), 0.0);
        }

        // 4. In-place Inverse FFT without scaling (1/M cancels in normalization)
        Fourier.Inverse(spectrum, FourierOptions.NoScaling);

        // 5. Normalize by lag 0 (M * variance)
        var varianceScaled = spectrum[0].Real;
        if (varianceScaled == 0.0)
        {
            return autocorrelations;
        }
        for (var lag = 1; lag < sample.Length; lag++)
        {
            autocorrelations[lag] = spectrum[lag].Real / varianceScaled;
        }
        return autocorrelations;
    }
}