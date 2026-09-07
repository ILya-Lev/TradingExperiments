namespace Udemy.FixedIncome;

public static class BondEstimators
{
    public static double GetPriceFlatTerms(
        double faceValue, double maturityYears, double yield,
        double coupon = 0,
        int compoundingFrequency = 1)
    {
        var y = yield >= 1 ? yield / 100.0 : yield;

        var totalDiscountFactor = Math.Pow(1 + y / compoundingFrequency, -compoundingFrequency * maturityYears);

        return (coupon / yield * (1 - totalDiscountFactor) + totalDiscountFactor) * faceValue;
    }
    
    public static double GetDollarDurationFlatTerms(
        double faceValue, double maturityYears, double yield,
        double coupon = 0,
        int compoundingFrequency = 1)
    {
        var y = yield >= 1 ? yield / 100.0 : yield;

        var totalDiscountFactor = Math.Pow(1 + y / compoundingFrequency, -compoundingFrequency * maturityYears);

        return (
                   coupon / yield / y * (1 - totalDiscountFactor)
                   - maturityYears * coupon / yield * totalDiscountFactor / (1 + y / compoundingFrequency)
                   + maturityYears * totalDiscountFactor / (1 + y / compoundingFrequency)
               )
               * faceValue;
    }
}

