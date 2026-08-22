namespace Udemy.FixedIncome;

public static class LinearInterpolator
{
    public const double Precision = 1e-6;

    public static (decimal a, decimal b)? GetLineFactors((decimal x, decimal y) lhs, (decimal x, decimal y) rhs)
    {
        //if (Math.Abs(lhs.x - rhs.x) < Precision) return null;//equation defining the curve in this case is x = lhs.x
        if (lhs.x == rhs.x) return null;

        var a = (rhs.y - lhs.y) / (rhs.x - lhs.x);
        var b = (rhs.x * lhs.y - rhs.y * lhs.x) / (rhs.x - lhs.x);

        return (a, b);
    }

    public static decimal GetY((decimal a, decimal b) lineFactors, decimal x) => lineFactors.a * x + lineFactors.b;

    public static IEnumerable<(double t, double v)> Interpolate(
        this IEnumerable<(double t, double v)> sparse
        , double step = 1.0)
    {
        var dense = new List<(double t, double v)>();

        foreach (var (t, v) in sparse)
        {
            if (dense.Count != 0 && Math.Abs(dense.Last().t + step - t) > Precision)
            {
                var lhs = ((decimal)dense.Last().t, (decimal)dense.Last().v);
                var rhs = ((decimal)t, (decimal)v);
                var lineFactors = GetLineFactors(lhs, rhs);
                for (var i = dense.Last().t + step; i < t; i += step)
                {
                    var interpolatedV = GetY(lineFactors!.Value, (decimal)i);
                    dense.Add((i, (double)interpolatedV));
                }
            }

            dense.Add((t, v));
        }

        return dense;
    }
}
