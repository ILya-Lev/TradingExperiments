namespace Udemy.FixedIncome;

public static class ForwardInterestRateCalculator
{
    public static double GetForwardRate(double r1, double t1, double r2, double t2, int compoundingFrequency = 1) =>
        compoundingFrequency * (
            Math.Pow((Math.Pow(1 + r2 / compoundingFrequency, t2))
                     / (Math.Pow(1 + r1 / compoundingFrequency, t1)),
                1.0 / (t2 - t1))
            - 1);

    public static Dictionary<double, double> GetImpliedSpotCurve(
        Dictionary<double, double> interestRatesPerYear
        , double t1
        , int compoundingFrequency = 1)
    {
        if(!interestRatesPerYear.TryGetValue(t1, out var r1))
            return [];

        var step = 1.0 / compoundingFrequency;
        var t2 = t1 + step;

        var impliedSpotCurve = new Dictionary<double, double>();

        while (interestRatesPerYear.TryGetValue(t2, out var r2))
        {
            var f = GetForwardRate(r1, t1, r2, t2, compoundingFrequency);
            impliedSpotCurve.Add(t2, f);
            t2 += step;
        }
        
        return impliedSpotCurve;
    }

    public static IEnumerable<(double t1, double t2, double f)> GetForwardRatesShifted(
        Dictionary<double, double> interestRatesPerYear
        , double shift = 1.0
        , int compoundingFrequency = 1)
        => interestRatesPerYear
            .Select(p =>
            {
                var t2 = p.Key + shift;
                if (!interestRatesPerYear.TryGetValue(t2, out var r2))
                    return default((double t1, double t2, double f)?);
                return (p.Key, t2, GetForwardRate(p.Value, p.Key, r2, t2, compoundingFrequency));
            })
            .Where(item => item is not null)
            .Select(item => item!.Value);
}
