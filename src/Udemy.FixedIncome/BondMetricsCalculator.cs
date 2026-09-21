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
}