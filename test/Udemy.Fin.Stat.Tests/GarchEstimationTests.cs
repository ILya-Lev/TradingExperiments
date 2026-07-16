using MathNet.Numerics.RootFinding;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class GarchEstimationTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("20030701", "20061231")]
    public async Task EstimateGarch_MethodOfMoments_Observe(string fromStr, string toStr)
    {
        var fromDate = DateOnly.ParseExact(fromStr, "yyyyMMdd");
        var toDate = DateOnly.ParseExact(toStr, "yyyyMMdd");
        var filter = (DateOnly d) => fromDate <= d && d <= toDate;

        var closeRices = await DemoHelpers.LoadClosePrices("GSPC", filter).ContinueWith(t => t.Result.ToArray());
        var logReturns = closeRices.Skip(1)
            .Zip(closeRices.SkipLast(1), (n, c) => Math.Log(n) - Math.Log(c))
            .ToArray();

        var moments = new MathNet.Numerics.Statistics.DescriptiveStatistics(logReturns, increasedAccuracy: true);
        var v = 100 * 100 * moments.Variance;
        var k = 3 + moments.Kurtosis;//as the stats give us excess kurtosis

        var squaredLogReturns = logReturns.Select(r => r * r).ToArray();
        var autocorrelationSeries = squaredLogReturns.GetAutoCorrelationParallel(50);
        //var m = CountSignificantAutocorrelations(autocorrelationSeries);
        var m = 20;
        var g = autocorrelationSeries.Skip(1).Take(m).Sum();

        var apb = (double a) => Math.Sqrt(1 - 2 * a * a * k / (k - 3));//i.e., (a+b)
        var maxA = Math.Sqrt((k - 3) / 2 / k);

        var omega = (double a) => v * (1 - apb(a));
        var beta = (double a) => apb(a) - a;
        var alpha = (double a) => (k - 3) / g / 3 / (k - 1)
                                  * (1 - (apb(a) - a) * apb(a))
                                  * (1 - Math.Pow(apb(a), m))
                                  / (1 - apb(a));

        var (convergedAlpha, stepsManual) = Converge(alpha, 0.1);
        var (convergedAlphaSecant, stepsSecant) = ConvergeSecant(alpha, 0.1);
        var convergedAlphaBrent = ConvergeBrent(alpha, -maxA, maxA);
        //var convergedAlphaBrent = double.NaN;
        var convergedBeta = beta(convergedAlpha);
        var convergedOmega = omega(convergedAlpha);

        output.WriteLine($"for S&P500 in date range {fromStr} to {toStr}");
        output.WriteLine($"sample moments: variance {v:N4}, kurtosis {k:N4}, first {m} autocorrelation sums {g}");
        output.WriteLine($"estimated GARCH(1,1) parameters: alpha {convergedAlpha}, beta {convergedBeta}, omega {convergedOmega}");
        output.WriteLine($"alpha manual {convergedAlpha:g4} vs Secant {convergedAlphaSecant:g4} vs Brent {convergedAlphaBrent:g4}, maxA {maxA:g4}");
        output.WriteLine($"alpha manual - Secant {convergedAlpha - convergedAlphaSecant:g4}; steps manual {stepsManual} vs Secant {stepsSecant}");
    }

    private static int CountSignificantAutocorrelations(double[] rhos)
    {
        var m = 2;
        while (m < rhos.Length)
        {
            if (Math.Abs(rhos[m - 2]) + Math.Abs(rhos[m - 1]) + Math.Abs(rhos[m]) < 9e-2)
                break;
            m++;
        }

        return m;
    }

    private static (double, int) Converge(Func<double, double> f, double seed)
    {
        var step = 0;

        var current = seed;
        var next = f(current);

        while (Math.Abs(next - current) > 1e-6)
        {
            current = next;
            next = f(current);

            if (!double.IsFinite(current) || !double.IsFinite(next))
                throw new InvalidOperationException($"diverges with seed {seed} as current {current} and next {next} at step {step}");

            if (double.IsNaN(current) || double.IsNaN(next))
                throw new InvalidOperationException($"diverges with seed {seed} as current {current} and next {next} at step {step}");

            if (step++ > 10_000)
                throw new InvalidOperationException($"cannot converge with seed {seed} within {step} steps");
        }

        return (next, step);
    }

    private static (double, int) ConvergeSecant(Func<double, double> f, double seed, int maxSteps = 100, double precision = 1e-6)
    {
        var step = 0;

        var x0 = seed;
        var x1 = f(x0);

        while (Math.Abs(x1 - x0) > precision)
        {
            if (step++ > maxSteps)
                throw new OverflowException($"Too many steps {step} vs {maxSteps}");

            var d0 = Delta(x0);
            var d1 = Delta(x1);
            if (Math.Abs(d1 - d0) < double.Epsilon)
                throw new InvalidOperationException($"Cannot divide by zero {d1 - d0:g2} < {double.Epsilon:g2}");

            var x2 = (d1 * x0 - d0 * x1) / (d1 - d0);

            x0 = x1;
            x1 = x2;
        }

        return (x1, step);

        double Delta(double x) => f(x) - x;
    }

    private static double ConvergeBrent(
        Func<double, double> f
        , double lowerBound = -100
        , double upperBound = 100
        , double accuracy = 1e-6)
    {
        return Brent.FindRoot(x => f(x) - x, lowerBound, upperBound, accuracy);
    }
}
