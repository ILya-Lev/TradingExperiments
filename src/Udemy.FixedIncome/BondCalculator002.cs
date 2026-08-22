namespace Udemy.FixedIncome;

public static class BondCalculator002
{
    public static double GetPrice(
        double faceValue
        , double maturityYears
        , double annualCouponPercentage
        , IReadOnlyDictionary<double, double> interestRates //r(T)
        , int compoundingFrequency = 1)
    {
        var c = annualCouponPercentage * faceValue / compoundingFrequency;
        var z = (double t) => Math.Pow(1 + interestRates[t] / compoundingFrequency, -compoundingFrequency * t);
        
        var price = faceValue * z(maturityYears);
        
        for (var i = 1; i <= maturityYears * compoundingFrequency; i++)
        {
            var t = i * 1.0 / compoundingFrequency;
            price += c * z(t);
        }

        return price;
    }
}
