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

        var sqrT = Math.Sqrt(T);

        D1 = Math.Abs(S - K) < 1e-6
            ? (R / Sigma + Sigma / 2) * sqrT
            : (Math.Log(S / K) + T * (R + Sigma * Sigma / 2)) / Sigma / sqrT;

        D2 = D1 - Sigma * sqrT;

        var nd2 = N(D2);
        var ndm2 = 1 - nd2;
        
        CallDelta = N(D1);
        PutDelta = CallDelta - 1;

        var discount = Math.Exp(-R * T);
        var discountedK = K * discount;

        //BSM call option price
        CallPrice = S * CallDelta - discountedK * nd2;

        //according to call-put european options parity 
        // c + k*exp(-r*t) = p + s
        PutPrice = CallPrice + discountedK - S;

        var nPrimeD1 = Math.Exp(-D1 * D1 / 2) / Math.Sqrt(2 * Math.PI);

        var thetaFirstPart = -S * nPrimeD1 * Sigma / 2 / sqrT;
        CallTheta = thetaFirstPart - R * discountedK * nd2;
        PutTheta = thetaFirstPart + R * discountedK * ndm2;

        Gamma = nPrimeD1 / Sigma / S / sqrT;

        Vega = S * sqrT * nPrimeD1;

        CallRho = T * discountedK * nd2;
        PutRho = -T * K / discount * ndm2;
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