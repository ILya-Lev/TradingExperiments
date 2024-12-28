namespace BlackScholesMerton.CameronConnellUdemy;

internal class PresentValueCalculator
{
    public static decimal GetPresentValue(
        Dictionary<decimal, (decimal r, decimal c)> paymentsAndInterestByPeriod,
        int compounding) => paymentsAndInterestByPeriod
        .Sum(pair =>
        {
            var time = pair.Key;
            var r = pair.Value.r;
            var c = pair.Value.c;

            var discount = 1 / Math.Pow(1 + (double)r / compounding, compounding * (double)time);

            return c * (decimal)discount;
        });

    public static decimal GetBondPrice(
        decimal faceValue,
        decimal couponRate,
        decimal flatInterestRate,
        int compounding,
        int maturity)
    {
        var discount = 1m;
        var aggregatedDiscount = 0m;
        for (int payment = 1; payment < compounding * maturity; payment++)
        {
            discount /= (1 + flatInterestRate / compounding);
            aggregatedDiscount += discount;
        }

        discount /= (1 + flatInterestRate / compounding);
        return faceValue
               * (aggregatedDiscount * couponRate / compounding
                  + (1 + couponRate / compounding) * discount);
    }
}