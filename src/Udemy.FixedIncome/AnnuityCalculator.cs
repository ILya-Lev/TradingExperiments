namespace Udemy.FixedIncome;

public readonly record struct Annuity(
    double Payment,
    int MaturityYears,
    double InterestRate,
    int CompoundingFrequency = 1);

public static class AnnuityCalculator
{
    extension(Annuity a)
    {
        //todo: review - there is some mistake in these formulas...
        public double GetPresentValue(bool inArrears = true, int deferredYears = 0)
        {
            var yearlyDiscount = Math.Pow(1 + a.InterestRate / a.CompoundingFrequency, -a.CompoundingFrequency);

            if (inArrears)
            {
                return a.Payment / a.InterestRate * a.CompoundingFrequency
                     * (Math.Pow(yearlyDiscount, deferredYears) - Math.Pow(yearlyDiscount, a.MaturityYears));
            }
            
            return a.Payment / (1-yearlyDiscount) 
                 * (Math.Pow(yearlyDiscount, deferredYears) - Math.Pow(yearlyDiscount, a.MaturityYears));
        }

        public double GetFutureValue(bool inArrears = true, int deferredYears = 0, int yearsAfter = 0)
        {
            var yearlyGrowth = Math.Pow(1 + a.InterestRate / a.CompoundingFrequency, a.CompoundingFrequency);

            return a.GetPresentValue(inArrears, deferredYears) 
                 * Math.Pow(yearlyGrowth, a.MaturityYears + deferredYears + yearsAfter);
        }
    }
}
