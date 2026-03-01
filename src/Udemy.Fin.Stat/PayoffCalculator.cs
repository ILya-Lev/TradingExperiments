namespace Udemy.Fin.Stat;

public static class PayoffCalculator
{
    public static decimal GetLongCall(decimal strike, decimal spotAtMaturity, decimal entrancePrice)
        => Math.Max(spotAtMaturity - strike, 0) - entrancePrice;
    
    public static decimal GetShortCall(decimal strike, decimal spotAtMaturity, decimal entrancePrice)
        => -GetLongCall(strike, spotAtMaturity, entrancePrice);

    public static decimal GetLongPut(decimal strike, decimal spotAtMaturity, decimal entrancePrice)
        => Math.Max(strike - spotAtMaturity, 0) - entrancePrice;

    public static decimal GetShortPut(decimal strike, decimal spotAtMaturity, decimal entrancePrice)
        => -GetLongPut(strike, spotAtMaturity, entrancePrice);

    public static decimal GetLongForward(decimal strike, decimal spotAtMaturity)
        => spotAtMaturity - strike;

    public static decimal GetShortForward(decimal strike, decimal spotAtMaturity)
        => -GetLongForward(strike, spotAtMaturity);
}
