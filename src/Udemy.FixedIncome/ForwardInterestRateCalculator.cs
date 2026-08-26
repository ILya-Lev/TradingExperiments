namespace Udemy.FixedIncome;

public static class ForwardInterestRateCalculator
{
    public static double GetForwardRate(double r1, double t1, double r2, double t2) => 
        Math.Pow((Math.Pow(1+r2, t2))/(Math.Pow(1+r1, t1)), 1 / (t2 - t1)) - 1;

    public static Dictionary<double, double> GetImpliedSpotCurve(Dictionary<double, double> interestRatesPerYear, double t1)
    {
        using var iterator = interestRatesPerYear.GetEnumerator();

        var r1 = 0.0;
        while (iterator.MoveNext())
        {
            if (Math.Abs(iterator.Current.Key - t1) < 1e-6)
            {
                r1 = iterator.Current.Value;
                break;
            }
        }

        if (r1 == 0) return [];
        
        var impliedSpotCurve = new Dictionary<double, double>();
        while (iterator.MoveNext())
        {
            var r2 = iterator.Current.Value;
            var t2 = iterator.Current.Key;

            var f = GetForwardRate(r1, t1, r2, t2);
            impliedSpotCurve.Add(t2,f);
        }

        return impliedSpotCurve;
    }

    public static IEnumerable<(double t1, double t2, double f)> GetOneYearForwardRates(Dictionary<double, double> interestRatesPerYear)
        => interestRatesPerYear.SkipLast(1)
            .Zip(interestRatesPerYear.Skip(1),
                (lhs, rhs) => (lhs.Key, rhs.Key, GetForwardRate(lhs.Value, lhs.Key, rhs.Value, rhs.Key)));
}
