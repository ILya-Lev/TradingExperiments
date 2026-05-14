using MathNet.Numerics.Distributions;
using System.Numerics.Tensors;

namespace Udemy.Fin.Stat;

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
        // ReSharper disable once IteratorNeverReturns
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

        // ReSharper disable once IteratorNeverReturns
    }
}
