using MathNet.Numerics.Distributions;

namespace BlackScholesMerton;

public class EuropeanOptionOnFuturesCalculator
{
    /// <summary>
    /// calculates fair option on futures price in Black76 model
    /// dependency over future price expected value is replaced with risk-free growth rate (bank - r)
    /// also see https://en.wikipedia.org/wiki/Black_model
    /// </summary>
    /// <param name="f">future price now</param>
    /// <param name="k">strike price (price of the asset at the exercise)</param>
    /// <param name="r">risk-free growth rate (bank account return rate)</param>
    /// <param name="sigma">standard deviation of future price distribution (its volatility square root)</param>
    /// <param name="t">time to maturity from now, in years</param>
    /// <remarks>fair price indicates whether to buy (if above) or to sell (if below the market)</remarks>
    public EuropeanOptionOnFuturesCalculator(double f, double k, double r, double sigma, double t)
    {
        F = f;
        K = k;
        R = r;
        Sigma = sigma;
        T = t;

        var sqrT = Math.Sqrt(T);
        var sigmaSqrT = Sigma * sqrT;

        D1 = Math.Abs(F - K) < 1e-6
            ? sigmaSqrT / 2
            : Math.Log(F / K) / sigmaSqrT + sigmaSqrT / 2;

        D2 = D1 - sigmaSqrT;

        var discount = Math.Exp(-R * T);

        //N(x) - 1 = -N(-x), for any x due to symmetry of normal distribution
        //calling N(x) is relatively expensive, so we calculate it once - difference is 70 ns vs 130 ns
        var nd1 = N(D1);
        var nd2 = N(D2);

        CallDelta = discount * nd1;
        PutDelta = discount * (nd1 - 1);

        //Black76 call option price; call/put parity works for equity, but not for future-based options
        CallPrice = discount * (F * nd1 - K * nd2);
        PutPrice = discount * (F * (nd1 - 1) - K * (nd2 - 1));
    }

    public double F { get; }
    public double K { get; }
    public double R { get; }
    public double Sigma { get; }
    public double T { get; }

    public double D1 { get; }
    public double D2 { get; }

    public double CallPrice { get; }
    public double PutPrice { get; }

    public double CallDelta { get; }
    public double PutDelta { get; }

    private static double N(double x) => Normal.CDF(0, 1, x);
}