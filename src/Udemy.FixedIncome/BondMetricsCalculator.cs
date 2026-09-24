namespace Udemy.FixedIncome;

/// <summary>
/// standard notations
/// r = interest rate in % - flat structure
/// t = maturity in years
/// f = face value in USD
/// c = coupon rate in %
/// k = compounding frequency (1 - annually, 2 - semiannually, 12 = monthly, 365 - daily - depends on the convention)
/// </summary>
public static class BondMetricsCalculator
{
    public static double GetCouponPayments(double r, int t, double f, double c, int k = 1)
        => f * c / r * (Math.Pow(1 + r / k, k * t) - 1);

    public static double GetFaceValue(double r, int t, double p, double c = 0, int k = 1)
        => p / (c / r + Math.Pow(1 + r / k, -k * t) * (1 - c / r));


    public static double GetBondPrice(double r, int t, double f, double c = 0, int k = 1)
        => f * (c / r + Math.Pow(1 + r / k, -k * t) * (1 - c / r));

    public static double GetBondDollarDuration(double r, int t, double f, double c = 0, int k = 1)
        => f * (Math.Pow(1 + r / k, -k * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / k, -k * t)));

    public static double GetBondDuration(double r, int t, double f, double c = 0, int k = 1)
        => GetBondDollarDuration(r, t, f, c, k) / GetBondPrice(r, t, f, c, k);

    public static double GetBondMacaulayDuration(double r, int t, double f, double c = 0, int k = 1)
        => GetBondDuration(r, t, f, c, k) * (1 + r);

    public static double GetBondDollarConvexity(double r, int t, double f, double c = 0, int k = 1)
        => f * (2 * c / r / r / r * (1 - Math.Pow(1 + r / k, -k * t)) - 2 * c * t / r / r * Math.Pow(1 + r / k, -k * t - 1) - t * (t + 1.0 / k) * (c / r - 1) * Math.Pow(1 + r / k, -k * t - 2));

    public static double GetBondConvexity(double r, int t, double f, double c = 0, int k = 1)
        => GetBondDollarConvexity(r, t, f, c, k) / GetBondPrice(r, t, f, c, k);

    public static (double lhs, double rhs) GetDollarConvexityHedgingScales
    (
        double c, double d,
        double c1, double d1,
        double c2, double d2
    )
    {
        var denominator = d1 * c2 - d2 * c1;
        var lhs = c * d2 - d * c2;
        var rhs = -(c * d1 - d * c1);

        return (lhs / denominator, rhs / denominator);
    }

    public static (double lhs, double rhs) GetConvexityHedgingScales
    (
        double[] prices, double[] durations, double[] convexity
    )
    {
        if (prices?.Length != 3 || durations?.Length != 3 || convexity?.Length != 3)
        {
            throw new ArgumentException($"each of the arrays should contain exactly 3 values in order");
        }

        var denominator = durations[1] * convexity[2] - durations[2] * convexity[1];
        var lhs = -durations[0] * convexity[2] + durations[2] * convexity[0];
        var rhs = durations[0] * convexity[1] - durations[1] * convexity[0];

        return 
        (
            prices[0] * lhs / prices[1] / denominator,
            prices[0] * rhs / prices[2] / denominator
        );
    }
}