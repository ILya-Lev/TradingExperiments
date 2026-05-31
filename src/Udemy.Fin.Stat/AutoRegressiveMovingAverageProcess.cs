using System.Numerics.Tensors;
using MathNet.Numerics.Distributions;

namespace Udemy.Fin.Stat;

// ReSharper disable IteratorNeverReturns
public static class AutoRegressiveMovingAverageProcess
{
    public static bool IsStationary(double phi1, double phi2) =>
        //phi1 * phi1 / 4 > Math.Abs(phi2)
        //&&
        Math.Abs(phi1) + Math.Sqrt(phi1 * phi1 + 4 * phi2) < 2;

    public static IEnumerable<double> GenerateArma(
        double[] phi,
        double[] theta,
        double stdDev = 1.0,
        double drift = 0.0)
    {
        var p = phi.Length;
        var q = theta.Length;
        var xHistory = new double[p];
        var epsHistory = new double[p];

        var normalGenerator = new Normal(0, stdDev);
        if (q > 0) normalGenerator.Samples(epsHistory);
        while (true)
        {
            var currentEps = normalGenerator.Sample();

            var arComponent = p > 0 ? TensorPrimitives.Dot(xHistory, phi) : 0;
            var maComponent = q > 0 ? TensorPrimitives.Dot(epsHistory, theta) : 0;

            var xt = drift + currentEps + arComponent + maComponent;
            yield return xt;

            if (p > 0)
            {
                xHistory.AsSpan(0, p - 1).CopyTo(xHistory.AsSpan(1));
                xHistory[0] = xt;
            }

            if (q > 0)
            {
                epsHistory.AsSpan(0, q - 1).CopyTo(epsHistory.AsSpan(1));
                epsHistory[0] = currentEps;
            }
        }
    }

    public static IEnumerable<double> GetAutoCorrelationFunction(double phi1, double phi2)
    {
        double roM2 = 1;
        yield return roM2;

        var roM1 = phi1 / (1 - phi2);
        yield return roM1;

        while (true)
        {
            var ro = phi1 * roM1 + phi2 * roM2;
            yield return ro;

            roM2 = roM1;
            roM1 = ro;
        }
    }

    public static IEnumerable<double> GenerateWhiteNoise(double stdDev = 1.0)
    {
        var normalGenerator = new Normal(0, stdDev);
        while (true)
        {
            yield return normalGenerator.Sample();
        }
    }
}
