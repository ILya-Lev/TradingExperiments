using MathNet.Numerics.Distributions;

namespace BlackScholesMerton;

public class EuropeanOptionOnFuturesCalculator
{
    /// <summary>
    /// calculates fair call option on futures price in Black76 model
    /// under risk-neutral probability Q
    /// i.e. dependency over future price expected value is replaced with risk-free growth rate (bank - r)
    /// as due to Girsanov theorem exists probability Q,
    /// under which Brownian motion W_Q = W + (mu - r)/sigma * t
    /// stock price is Martingale, i.e. E_Q[exp{-r*(T-t)}*S(T)] = S(t)
    /// also see https://en.wikipedia.org/wiki/Black_model
    /// </summary>
    /// <param name="f">future price now</param>
    /// <param name="k">strike price (price of the asset at the exercise)</param>
    /// <param name="r">risk-free growth rate (bank account return rate)</param>
    /// <param name="sigma">standard deviation of future price distribution (its volatility square root)</param>
    /// <param name="t">time to maturity from now, in years</param>
    /// <remarks>fair price indicates whether to buy (if above) or to sell (if below market)</remarks>
    public EuropeanOptionOnFuturesCalculator(double f, double k, double r, double sigma, double t)
    {
        F = f;
        K = k;
        R = r;
        Sigma = sigma;
        T = t;

        var sqrT = Math.Sqrt(T);

        D1 = Math.Abs(F - K) < 1e-6
            ? Sigma / 2 * sqrT
            : Math.Log(F/K) /Sigma/sqrT + sqrT*Sigma/2;

        D2 = D1 - Sigma * sqrT;

        var nd2 = N(D2);
        var ndm2 = 1 - nd2;
        
        CallDelta = N(D1);
        PutDelta = CallDelta - 1;

        var discount = Math.Exp(-R * T);
        var discountedK = K * discount;

        //Black76 call option price
        CallPrice = discount * (F * CallDelta - K * nd2);

        //according to call-put european options parity 
        // c + k*exp(-r*t) = p + s
        PutPrice = CallPrice + discountedK - F;
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