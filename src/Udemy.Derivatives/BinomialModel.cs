namespace Udemy.Derivatives;

public class BinomialModel
{
    public BinomialModel(double up, double down, double interestRate)
    {
        if (up < 1 + interestRate)
            throw new ArgumentException($"arbitrage constraint 1+r < u is violated: 1+{interestRate} > {up}");
        if (1 + interestRate < down)
            throw new ArgumentException($"arbitrage constraint d < 1+r is violated: {down} > 1+{interestRate}");
        if (down < 0)
            throw new ArgumentException($"d < 0: {down} < 0");

        Up = up;
        Down = down;
        InterestRate = interestRate;

        RiskFreeUp = (1 + interestRate - down) / (up - down);
        RiskFreeDown = (up - 1 - interestRate) / (up - down);
    }

    public double Up { get; }
    public double Down { get; }
    public double RiskFreeUp { get; }
    public double RiskFreeDown { get; }
    public double InterestRate { get; }

    public double GetInitialDerivativePrice(IReadOnlyList<double> finalPayoffs)
    {
        var levels = finalPayoffs.Count - 1;
        var initialDerivativePrice = finalPayoffs
            .Select((d, i) => d 
                            * Math.Pow(RiskFreeUp, levels - i)
                            * Math.Pow(RiskFreeDown, i)
                            * GetBinomialFactor(levels, i))
            .Sum();

        initialDerivativePrice /= Math.Pow(1 + InterestRate, levels);
        return initialDerivativePrice;
    }

    public double GetInitialCallPrice(double initialAssetPrice, double strike, int levels)
    {
        var lowerLimit = (int)Math.Ceiling( (Math.Log(strike/initialAssetPrice) - levels * Math.Log(Down)) / Math.Log(Up / Down));
        lowerLimit = Math.Max(0, lowerLimit);//should always be > 0, but there are edge cases of Down > 1 and n -> infinity

        var initialCallPrice = 0.0;
        for (var j = lowerLimit; j <= levels; j++)
        {
            initialCallPrice += GetBinomialFactor(levels, j)
                                * Math.Pow(RiskFreeUp, j)
                                * Math.Pow(RiskFreeDown, levels - j)
                                * (Math.Pow(Up, j) * Math.Pow(Down, levels - j) * initialAssetPrice - strike);
        }

        initialCallPrice /= Math.Pow(1 + InterestRate, levels);
        return initialCallPrice;
    }

    private static double GetBinomialFactor(int n, int i)
    {
        //n ! / i! / (n-i)!
        var result = 1.0;
        for (var j = 1; j < i+1; j++)
        {
            result *= (n - i + j) *1.0 / j;
        }
        return result;
    }
}
