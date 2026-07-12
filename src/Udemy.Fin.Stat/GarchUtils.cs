using MathNet.Numerics.Distributions;
using ScottPlot;

namespace Udemy.Fin.Stat;

public static class GarchUtils
{
    public static double Beta(double alpha, double kurtosis)
    {
        return Math.Sqrt(1.0 - 2.0 * alpha * alpha * kurtosis / (kurtosis - 3.0)) - alpha;
    }

    public static double Gamma(double alpha, double kurtosis, int n)
    {
        double beta1 = Beta(alpha, kurtosis);

        double factor1 = alpha * (1.0 - beta1 * (alpha + beta1));
        double factor2 = 1.0 - Math.Pow(alpha + beta1, 2) + alpha * alpha;
        double factor3 = 1.0 - Math.Pow(alpha + beta1, n);
        double factor4 = 1.0 - alpha - beta1;

        return factor1 * factor3 / factor2 / factor4;
    }

    /// <summary>
    /// Fits the GARCH parameters using the Secant method.
    /// </summary>
    public static (double Alpha, double Beta) Fit(double gammaValue, double kurtosis, int n)
    {
        const double tolerance = 0.000001;
        double bound = Math.Sqrt((kurtosis - 3.0) / ((kurtosis - 1.0) * 3.0));

        double alphaOld = 0.95 * bound;
        double alphaTrial = bound / 2.0;

        while (Gamma(alphaTrial, kurtosis, n) < gammaValue)
        {
            alphaOld = alphaTrial;
            alphaTrial /= 2.0;
        }

        double alphaNew = alphaTrial;

        while (Math.Abs(alphaNew - alphaOld) > tolerance)
        {
            double qOld = Gamma(alphaOld, kurtosis, n) - gammaValue;
            double qNew = Gamma(alphaNew, kurtosis, n) - gammaValue;

            double alpha = alphaNew - qNew * (alphaNew - alphaOld) / (qNew - qOld);
            alphaOld = alphaNew;
            alphaNew = alpha;
        }

        double beta1 = Beta(alphaNew, kurtosis);
        return (alphaNew, beta1);
    }

    /// <summary>
    /// Plots the Gamma function fit against the target gamma value.
    /// </summary>
    public static void FitPlot(double gammaValue, double kurtosis, int n, Plot? plot = null)
    {
        plot ??= new Plot(); // Create a new plot if none is provided

        double bound = Math.Sqrt((kurtosis - 3.0) / ((kurtosis - 1.0) * 3.0));
        int upper = (int)(bound * 100);

        // Generate arrays using C# 14 / LINQ
        double[] xVals = Enumerable.Range(1, upper - 1).Select(i => i / 100.0).ToArray();
        double[] yVals = xVals.Select(x => Gamma(x, kurtosis, n)).ToArray();
        double[] gVals = Enumerable.Repeat(gammaValue, xVals.Length).ToArray();

        plot.Add.ScatterLine(xVals, yVals);
        plot.Add.ScatterLine(xVals, gVals);
    }

    /// <summary>
    /// Simulates a GARCH(1,1) process.
    /// </summary>
    public static double[] Simulate(double alpha, double beta, double omega, int max = 1000, Plot? plot = null)
    {
        var normal = new Normal(0.0, 1.0);

        double[] eps = new double[max];
        double[] sigmaSquared = new double[max];

        // Initialization
        eps[0] = normal.Sample();
        sigmaSquared[0] = omega / (1.0 - alpha - beta);

        for (int i = 1; i < max; i++)
        {
            double sigma = Math.Sqrt(omega + alpha * eps[i - 1] * eps[i - 1] + beta * sigmaSquared[i - 1]);
            eps[i] = sigma * normal.Sample();
            sigmaSquared[i] = sigma * sigma;
        }

        if (plot != null)
        {
            double[] xIndices = Enumerable.Range(0, max).Select(x => (double)x).ToArray();
            plot.Add.ScatterLine(xIndices, eps);
        }

        return eps;
    }
}
