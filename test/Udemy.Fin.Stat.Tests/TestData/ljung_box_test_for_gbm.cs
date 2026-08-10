using System;
using System.Numerics.Tensors;
using MathNet.Numerics.Distributions;

public static class LjungBox
{
    /// <summary>
    /// Executes the Ljung-Box test on a sequence of log returns.
    /// </summary>
    /// <param name="returns">Stationary time series data (e.g., log returns).</param>
    /// <param name="lags">Number of lags to test (typically ln(n) or 10-20).</param>
    /// <returns>A tuple containing the Q-statistic and the p-value.</returns>
    public static (double QStat, double PValue) Test(ReadOnlySpan<double> returns, int lags)
    {
        int n = returns.Length;
        double mean = TensorPrimitives.Average(returns);
        
        // Calculate the denominator: sum of squared deviations from the mean
        double varianceSum = 0;
        foreach (double r in returns)
        {
            double diff = r - mean;
            varianceSum += diff * diff;
        }

        double qStat = 0;

        // Calculate autocorrelation for each lag and accumulate Q-statistic
        for (int k = 1; k <= lags; k++)
        {
            double covarSum = 0;
            
            // Vectorizable loop for covariance at lag k
            for (int t = k; t < n; t++)
            {
                covarSum += (returns[t] - mean) * (returns[t - k] - mean);
            }

            double rhoK = covarSum / varianceSum;
            qStat += (rhoK * rhoK) / (n - k);
        }
        

        qStat *= n * (n + 2);

        // p-value is the upper tail probability of the Chi-Square distribution
        double pValue = 1.0 - ChiSquared.CDF(lags, qStat);

        return (qStat, pValue);
    }

    /// <summary>
    /// Helper to convert prices to log returns.
    /// </summary>
    public static double[] CalculateLogReturns(ReadOnlySpan<double> prices)
    {
        double[] returns = new double[prices.Length - 1];
        for (int i = 0; i < returns.Length; i++)
        {
            returns[i] = Math.Log(prices[i + 1] / prices[i]);
        }
        return returns;
    }
}

// ==========================================
// EXAMPLE USAGE:
// ==========================================
public static class SimulationRunner
{
    public static void Run()
    {
        // 1. Simulate 1000 days of S&P500 using GBM (LogNormal)
        int days = 1000;
        double[] prices = new double[days];
        prices[0] = 5000.0; // Starting S&P500 value
        
        var normal = new Normal(0, 1);
        double mu = 0.08 / 252;    // Daily drift (~8% annual)
        double sigma = 0.15 / Math.Sqrt(252); // Daily volatility (~15% annual)

        for (int i = 1; i < days; i++)
        {
            double z = normal.Sample();
            prices[i] = prices[i - 1] * Math.Exp((mu - 0.5 * sigma * sigma) + sigma * z);
        }

        // 2. Convert to log returns
        double[] logReturns = LjungBox.CalculateLogReturns(prices);

        // 3. Run Ljung-Box test for 10 lags
        int lags = 10;
        var (qStat, pValue) = LjungBox.Test(logReturns, lags);

        Console.WriteLine($"Q-Statistic: {qStat:F4}");
        Console.WriteLine($"P-Value: {pValue:F4}");
        
        if (pValue < 0.05)
            Console.WriteLine("Result: Reject H0. Significant serial correlation detected.");
        else
            Console.WriteLine("Result: Fail to reject H0. Series appears to be independent (White Noise).");
    }
}