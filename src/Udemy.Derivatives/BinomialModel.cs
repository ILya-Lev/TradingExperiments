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

        RiskFreeUp = (1 + interestRate - down) / (up - down);
        RiskFreeDown = (up - 1 - interestRate) / (up - down);
        InterestRate = interestRate;
    }

    public double RiskFreeUp { get; }
    public double RiskFreeDown { get; }
    public double InterestRate { get; }

    public double GetInitialDerivativePrice(IReadOnlyList<double> finalPayoffs)
    {
        var levels = finalPayoffs.Count - 1;
        var d0 = finalPayoffs
            .Select((d, i) => d 
                            * Math.Pow(RiskFreeUp, levels - i)
                            * Math.Pow(RiskFreeDown, i)
                            * GetBinomialFactor(levels, i))
            .Sum();

        d0 /= Math.Pow(1 + InterestRate, levels);
        return d0;
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
