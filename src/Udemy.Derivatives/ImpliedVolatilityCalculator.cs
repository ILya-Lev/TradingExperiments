using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace Udemy.Derivatives;

public class ImpliedVolatilityCalculator
{
    public double Spot { get; }
    public double InterestRate { get; }

    public ImpliedVolatilityCalculator(double spot, double interestRate)
    {
        Spot = spot;
        InterestRate = interestRate;
    }

    public double GetImpliedVolatility(double callPrice, double strike, double timeToMaturity)
    {
        var dPlus = GetDPlus(strike, timeToMaturity);
        var dMinus = GetDMinus(strike, timeToMaturity);

        var priceForVolatility = (double volatility)
            => Spot * Normal.CDF(0, 1, dPlus(volatility))
               - strike * Math.Exp(-InterestRate * timeToMaturity)
                        * Normal.CDF(0, 1, dMinus(volatility));

        var f = (double volatility) => callPrice - priceForVolatility(volatility);

        const double lowerBound = 0.01, upperBound = 0.99, accuracy = 1e-3;
        var impliedVolatility = FindRoots.OfFunction(f, lowerBound, upperBound, accuracy, 100);

        return impliedVolatility;
    }

    public double GetImpliedVolatilityByPut(double putPrice, double strike, double timeToMaturity)
    {
        var dPlus = GetDPlus(strike, timeToMaturity);
        var dMinus = GetDMinus(strike, timeToMaturity);

        var priceForVolatility = (double volatility)
            => Spot * (Normal.CDF(0, 1, dPlus(volatility)) - 1)
               - strike * Math.Exp(-InterestRate * timeToMaturity)
                        * (Normal.CDF(0, 1, dMinus(volatility)) - 1);

        var f = (double volatility) => putPrice - priceForVolatility(volatility);

        const double lowerBound = 0.01, upperBound = 0.99, accuracy = 1e-3;
        var impliedVolatility = FindRoots.OfFunction(f, lowerBound, upperBound, accuracy, 100);

        return impliedVolatility;
    }

    private Func<double, double> GetDMinus(double strike, double timeToMaturity)
        => volatility
            => GetDPlus(strike, timeToMaturity)(volatility) - volatility * Math.Sqrt(timeToMaturity);

    private Func<double, double> GetDPlus(double strike, double timeToMaturity)
        => volatility
            => (Math.Log(Spot / strike) + (InterestRate + volatility * volatility / 2) * timeToMaturity)
               / volatility
               / Math.Sqrt(timeToMaturity);

    private static double GetNPrime(double dPlus)
        => Math.Exp(-dPlus * dPlus / 2) / Math.Sqrt(2 * Math.PI);
}