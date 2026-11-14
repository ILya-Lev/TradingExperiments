using MathNet.Numerics.Distributions;

namespace Udemy.Derivatives;

public record Option(double Strike, double MaturityYears);

public record CallOption(double Strike, double MaturityYears) : Option(Strike, MaturityYears);

public record PutOption(double Strike, double MaturityYears) : Option(Strike, MaturityYears);

public interface IBlackScholesModel
{
    double GetPrice(Option option, double timeToMaturity, double? spot = null);
    double GetDelta(Option option, double timeToMaturity, double? spot = null);
    double GetGamma(Option option, double timeToMaturity, double? spot = null);
    double GetVega(Option option, double timeToMaturity, double? spot = null);
    double GetTheta(Option option, double timeToMaturity, double? spot = null);
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
    public double GetPrice(Option option, double timeToMaturity, double? spot = null)
        => _modelsByType[option.GetType()].GetPrice(option, timeToMaturity, spot);

    public double GetDelta(Option option, double timeToMaturity, double? spot = null)
        => _modelsByType[option.GetType()].GetDelta(option, timeToMaturity, spot);

    public double GetGamma(Option option, double timeToMaturity, double? spot = null)
    => _modelsByType[option.GetType()].GetGamma(option, timeToMaturity, spot);

    public double GetVega(Option option, double timeToMaturity, double? spot = null)
        => _modelsByType[option.GetType()].GetVega(option, timeToMaturity, spot);

    public double GetTheta(Option option, double timeToMaturity, double? spot = null)
        => _modelsByType[option.GetType()].GetTheta(option, timeToMaturity, spot);


    private double GetDMinus(double strike, double timeToMaturity, double? spot = null)
        => GetDPlus(strike, timeToMaturity, spot) - Volatility * Math.Sqrt(timeToMaturity);

    private double GetDPlus(double strike, double timeToMaturity, double? spot = null)
        => (Math.Log((spot ?? Spot) / strike) + (InterestRate + Volatility * Volatility / 2) * timeToMaturity)
           / Volatility
           / Math.Sqrt(timeToMaturity);

    private static double GetNPrime(double dPlus)
        => Math.Exp(-dPlus * dPlus / 2) / Math.Sqrt(2 * Math.PI);
}
internal class BlackScholesModelCall(
    double spot,
    double interestRate,
    double volatility,
    Func<double, double, double?, double> dPlusGenerator,
    Func<double, double, double?, double> dMinusGenerator,
    Func<double, double> nPrimeGenerator) : IBlackScholesModel
{
    public double Spot { get; } = spot;
    public double InterestRate { get; } = interestRate;
    public double Volatility { get; } = volatility;

    /// <summary>
    /// if the option expires in 6M, and you want to know the price now (t=0), provide 6M = 0.5
    /// if you want to know the price in 1M, provide 5M = 5/12.0
    /// </summary>
    public double GetPrice(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity, spot);

        return (spot ?? Spot) * Normal.CDF(0, 1, dPlus)
               - option.Strike * Math.Exp(-InterestRate * timeToMaturity) * Normal.CDF(0, 1, dMinus);
    }

    public double GetDelta(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        return Normal.CDF(0, 1, dPlus);
    }

    public double GetGamma(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var nPrime = nPrimeGenerator(dPlus);
        return nPrime / Volatility / (spot ?? Spot) / Math.Sqrt(timeToMaturity);
    }

    public double GetVega(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var nPrime = nPrimeGenerator(dPlus);
        return (spot ?? Spot) * Math.Sqrt(timeToMaturity) * nPrime;
    }

    public double GetTheta(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity, spot);
        var nPrime = nPrimeGenerator(dPlus);

        return -(spot ?? Spot) * Volatility * nPrime / 2 / Math.Sqrt(timeToMaturity)
               - option.Strike * InterestRate * Math.Exp(-InterestRate * timeToMaturity)
               * Normal.CDF(0, 1, dMinus);
    }
}

internal class BlackScholesModelPut(
    double spot,
    double interestRate,
    double volatility,
    Func<double, double, double?, double> dPlusGenerator,
    Func<double, double, double?, double> dMinusGenerator,
    Func<double, double> nPrimeGenerator) : IBlackScholesModel
{
    public double Spot { get; } = spot;
    public double InterestRate { get; } = interestRate;
    public double Volatility { get; } = volatility;

    /// <summary>
    /// if the option expires in 6M, and you want to know the price now (t=0), provide 6M = 0.5
    /// if you want to know the price in 1M, provide 5M = 5/12.0
    /// </summary>
    public double GetPrice(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity, spot);

        return -(spot ?? Spot) * Normal.CDF(0, 1, -dPlus)
               + option.Strike * Math.Exp(-InterestRate * timeToMaturity) * Normal.CDF(0, 1, -dMinus);
    }

    public double GetDelta(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        return Normal.CDF(0, 1, dPlus) - 1;
    }

    public double GetGamma(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var nPrime = nPrimeGenerator(dPlus);
        return nPrime / (spot ?? Spot) / Volatility / Math.Sqrt(timeToMaturity);
    }

    public double GetVega(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var nPrime = nPrimeGenerator(dPlus);
        return (spot ?? Spot) * nPrime * Math.Sqrt(timeToMaturity);
    }

    public double GetTheta(Option option, double timeToMaturity, double? spot = null)
    {
        var dPlus = dPlusGenerator(option.Strike, timeToMaturity, spot);
        var dMinus = dMinusGenerator(option.Strike, timeToMaturity, spot);
        var nPrime = nPrimeGenerator(dPlus);

        return -(spot ?? Spot) * nPrime * Volatility / 2 / Math.Sqrt(timeToMaturity)
               - option.Strike * InterestRate * Math.Exp(-InterestRate * timeToMaturity)
               * (Normal.CDF(0, 1, dMinus) - 1);
    }
}