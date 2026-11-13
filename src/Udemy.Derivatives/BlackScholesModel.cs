using MathNet.Numerics.Distributions;

namespace Udemy.Derivatives;

public record Option(double Strike, double MaturityYears);

public record CallOption(double Strike, double MaturityYears) : Option(Strike, MaturityYears);

public record PutOption(double Strike, double MaturityYears) : Option(Strike, MaturityYears);

public interface IBlackScholesModel
{
    double GetPrice(Option option, double timeToMaturity);
    double GetDelta(Option option, double timeToMaturity);
    double GetGamma(Option option, double timeToMaturity);
    double GetVega(Option option, double timeToMaturity);
    double GetTheta(Option option, double timeToMaturity);
}

public class BlackScholesModel : IBlackScholesModel
{
    private readonly IReadOnlyDictionary<Type, IBlackScholesModel> _modelsByType;

    public BlackScholesModel(double spot, double interestRate, double volatility)
    {
        Spot = spot;
        InterestRate = interestRate;
        Volatility = volatility;
        _modelsByType = new Dictionary<Type, IBlackScholesModel>()
        {
            [typeof(CallOption)] = new BlackScholesModelCall(spot, interestRate, volatility, GetDPlus, GetDMinus, GetNPrime),
            [typeof(PutOption)] = new BlackScholesModelPut(spot, interestRate, volatility, GetDPlus, GetDMinus, GetNPrime)
        };
    }

    public double Spot { get; }
    public double InterestRate { get; }
    public double Volatility { get; }

    /// <summary>
    /// if the option expires in 6M, and you want to know the price now (t=0), provide 6M = 0.5
    /// if you want to know the price in 1M, provide 5M = 5/12.0
    /// </summary>
    public double GetPrice(Option option, double timeToMaturity)
        => _modelsByType[option.GetType()].GetPrice(option, timeToMaturity);

    public double GetDelta(Option option, double timeToMaturity)
        => _modelsByType[option.GetType()].GetDelta(option, timeToMaturity);

    public double GetGamma(Option option, double timeToMaturity)
    => _modelsByType[option.GetType()].GetGamma(option, timeToMaturity);

    public double GetVega(Option option, double timeToMaturity)
        => _modelsByType[option.GetType()].GetVega(option, timeToMaturity);

    public double GetTheta(Option option, double timeToMaturity)
        => _modelsByType[option.GetType()].GetTheta(option, timeToMaturity);


    private double GetDMinus(double strike, double timeToMaturity)
        => GetDPlus(strike, timeToMaturity) - Volatility * Math.Sqrt(timeToMaturity);

    private double GetDPlus(double strike, double timeToMaturity)
        => (Math.Log(Spot / strike) + (InterestRate + Volatility * Volatility / 2) * timeToMaturity)
           / Volatility
           / Math.Sqrt(timeToMaturity);

    private static double GetNPrime(double dPlus)
        => Math.Exp(-dPlus * dPlus / 2) / Math.Sqrt(2 * Math.PI);
}
internal class BlackScholesModelCall(
    double spot,
    double interestRate,
    double volatility,
    Func<double, double, double> dPlusGenerator,
    Func<double, double, double> dMinusGenerator,
    Func<double, double> nPrimeGenerator) : IBlackScholesModel
{
    public double Spot { get; } = spot;
    public double InterestRate { get; } = interestRate;
    public double Volatility { get; } = volatility;

    /// <summary>
    /// if the option expires in 6M, and you want to know the price now (t=0), provide 6M = 0.5
    /// if you want to know the price in 1M, provide 5M = 5/12.0
    /// </summary>
    public double GetPrice(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity);

        return Spot * Normal.CDF(0, 1, dPlus)
               - option.Strike * Math.Exp(-InterestRate * timeToMaturity) * Normal.CDF(0, 1, dMinus);
    }

    public double GetDelta(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        return Normal.CDF(0, 1, dPlus);
    }

    public double GetGamma(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var nPrime = nPrimeGenerator(dPlus);
        return nPrime / Volatility / Spot / Math.Sqrt(timeToMaturity);
    }

    public double GetVega(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var nPrime = nPrimeGenerator(dPlus);
        return Spot * Math.Sqrt(timeToMaturity) * nPrime;
    }

    public double GetTheta(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity);
        var nPrime = nPrimeGenerator(dPlus);

        return -Spot * Volatility * nPrime / 2 / Math.Sqrt(timeToMaturity)
               - option.Strike * InterestRate * Math.Exp(-InterestRate * timeToMaturity)
               * Normal.CDF(0, 1, dMinus);
    }
}

internal class BlackScholesModelPut(
    double spot,
    double interestRate, 
    double volatility,
    Func<double, double, double> dPlusGenerator,
    Func<double, double, double> dMinusGenerator,
    Func<double, double> nPrimeGenerator) : IBlackScholesModel
{
    public double Spot { get; } = spot;
    public double InterestRate { get; } = interestRate;
    public double Volatility { get; } = volatility;

    /// <summary>
    /// if the option expires in 6M, and you want to know the price now (t=0), provide 6M = 0.5
    /// if you want to know the price in 1M, provide 5M = 5/12.0
    /// </summary>
    public double GetPrice(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity);

        return -Spot * Normal.CDF(0, 1, -dPlus)
               + option.Strike * Math.Exp(-InterestRate * timeToMaturity) * Normal.CDF(0, 1, -dMinus);
    }

    public double GetDelta(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        return Normal.CDF(0, 1, dPlus) - 1;
    }

    public double GetGamma(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var nPrime = nPrimeGenerator(dPlus);
        return nPrime / Spot / Volatility / Math.Sqrt(timeToMaturity);
    }

    public double GetVega(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var nPrime = nPrimeGenerator(dPlus);
        return Spot * nPrime * Math.Sqrt(timeToMaturity);
    }

    public double GetTheta(Option option, double timeToMaturity)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity);
        var nPrime = nPrimeGenerator(dPlus);

        return -Spot * nPrime * Volatility / 2 / Math.Sqrt(timeToMaturity)
               - option.Strike * InterestRate * Math.Exp(-InterestRate * timeToMaturity)
               * (Normal.CDF(0, 1, dMinus) - 1);
    }
}