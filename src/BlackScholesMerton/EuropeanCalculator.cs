using MathNet.Numerics.Distributions;

namespace BlackScholesMerton;

public class EuropeanCalculator
{
    /// <summary>
    /// calculates fair call option price in Black-Scholes-Merton model
    /// under risk-neutral probability Q
    /// i.e. dependency over stock price expected value is replaced with risk-free growth rate (bank - r)
    /// as due to Girsanov theorem exists probability Q,
    /// under which Brownian motion W_Q = W + (mu - r)/sigma * t
    /// stock price is Martingale, i.e. E_Q[exp{-r*(T-t)}*S(T)] = S(t)
    /// </summary>
    /// <param name="s">stock price now</param>
    /// <param name="k">strike price (price of the asset at the exercise)</param>
    /// <param name="r">risk-free growth rate (bank account return rate)</param>
    /// <param name="sigma">standard deviation of stock price distribution (its volatility square root)</param>
    /// <param name="t">time to maturity from now, in years</param>
    /// <remarks>fair price indicates whether to buy (if above) or to sell (if below market)</remarks>
    public EuropeanCalculator(double s, double k, double r, double sigma, double t)
    {
        S = s;
        K = k;
        R = r;
        Sigma = sigma;
        T = t;

        D1 = Math.Abs(s - k) < 1e-6
            ? (r / sigma + sigma / 2) * Math.Sqrt(t)
            : (Math.Log(s / k) + t * (r + sigma * sigma / 2)) / sigma / Math.Sqrt(t);

        D2 = D1 - sigma * Math.Sqrt(t);

        //BSM call option price
        CallPrice = S * N(D1) - K * Math.Exp(-R * T) * N(D2);

        //according to call-put european options parity 
        // c + k*exp(-r*t) = p + s
        PutPrice = CallPrice + K * Math.Exp(-R * T) - S;

        CallDelta = N(D1);
        PutDelta = N(D1) - 1;

        var nPrimeD1 = Math.Exp(-D1 * D1 / 2) / Math.Sqrt(2 * Math.PI);

        var thetaFirstPart = -S * nPrimeD1 * Sigma / 2 / Math.Sqrt(T);
        CallTheta = thetaFirstPart - R * K * Math.Exp(-R * T) * N(D2);
        PutTheta = thetaFirstPart + R * K * Math.Exp(-R * T) * N(-D2);

        Gamma = nPrimeD1 / Sigma / S / Math.Sqrt(T);

        Vega = S * Math.Sqrt(T) * nPrimeD1;

        CallRho = K * T * Math.Exp(-R * T) * N(D2);
        PutRho = -K * T * Math.Exp(R * T) * N(-D2);
    }

    public double S { get; }
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
    public double CallTheta { get; }
    public double PutTheta { get; }
    public double Gamma { get; }
    public double Vega { get; }
    public double CallRho { get; }
    public double PutRho { get; }

    private static double N(double x) => Normal.CDF(0, 1, x);

}