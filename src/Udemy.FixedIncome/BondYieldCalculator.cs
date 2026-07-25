namespace Udemy.FixedIncome;

public readonly record struct Bond(
    decimal FaceValue,
    decimal CouponPercentage,
    int CouponCompounding,
    decimal Yield,
    int YieldCompounding,
    decimal MaturityYears);

public static class BondYieldCalculator
{
    extension(Bond bond)
    {
        public IEnumerable<(decimal t, decimal c)> GetCashFlow()
        {
            var step = 1m / bond.CouponCompounding;
            var coupon = bond.CouponPercentage * bond.FaceValue / bond.CouponCompounding;
            for (var t = step; t < bond.MaturityYears; t += step)
            {
                yield return (t, coupon);
            }
            yield return (bond.MaturityYears, coupon + bond.FaceValue);
        }

        public decimal GetPrice()
            => bond.GetCashFlow()
                .Sum(item => item.c / (decimal)Math.Pow(1 + (double)bond.Yield / bond.YieldCompounding, (double)item.t * bond.YieldCompounding));
    }
}
