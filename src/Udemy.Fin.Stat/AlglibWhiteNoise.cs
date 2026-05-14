using MathNet.Numerics.Distributions;
using System.Numerics.Tensors;

namespace Udemy.Fin.Stat;

// ReSharper disable IteratorNeverReturns
public static class AlgLibWhiteNoise
{
    public static IEnumerable<double> GenerateWhiteNoise(double mean = 0, double stdDev = 1.0)
    {
        //seeds the generator with the current time/entropy. For determinism in testing use:
        //alglib.hqrndseed(1,2,out var state);
        //hqrnd = high quality random
        alglib.hqrndrandomize(out var state);

        while(true)
        {
            yield return alglib.hqrndnormal(state) * stdDev + mean;
        }
    }

    public static IEnumerable<double> GenerateRandomWalk(IEnumerable<double> whiteNoise)
    {
        var current = 0.0;
        foreach (var step in whiteNoise)
        {
            current += step;
            yield return current;
        }
    }


    /// <summary>
    /// returns moving average simulation
    /// X_t = e_t + theta * e_(t-1)
    /// where e_t - epsilon (eps for short) ~ N(0, stdDev)
    /// the product could be a dot product of 2 vectors - theta and epsilon
    /// in case of MA(1) it is just one point.
    /// </summary>
    /// <param name="theta">history weighting vector</param>
    /// <param name="stdDev">standard deviation for the underlying normal distribution</param>
    public static IEnumerable<double> GetMaSimulation(double[] theta, double stdDev = 1.0)
    {
        var eps = new double[theta.Length];
        var normalGenerator = new Normal(0.0, stdDev);
        normalGenerator.Samples(eps);

        while (true)
        {
            var currentEps = normalGenerator.Sample();

            yield return currentEps + ScalarProduct();

            eps.AsSpan(0, theta.Length - 1).CopyTo(eps.AsSpan(1));
            eps[0] = currentEps;
        }

        double ScalarProduct() => theta.Length >= 64
            ? TensorPrimitives.Dot(theta, eps)
            : theta.Zip(eps, (t, e) => t * e).Sum();
    }

    public static IEnumerable<double> GetArSimulation(double fi0, double fi1, double stdDev = 1.0)
    {
        //if (Math.Abs(fi1) >= 1.0)
        //    throw new InvalidOperationException($"fi1 must be within (-1; 1), but provided {fi1}");

        var normalGenerator = new Normal(0.0, stdDev);
        var previous = fi0 + normalGenerator.Sample();

        while (true)
        {
            var current = fi0 + fi1 * previous + normalGenerator.Sample();
            
            yield return current;

            previous = current;
        }
    }
}
