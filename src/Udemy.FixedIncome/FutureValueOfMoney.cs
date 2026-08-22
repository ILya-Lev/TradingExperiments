namespace Udemy.FixedIncome;

public static class FutureValueOfMoney
{
    public static double GetFutureValue(this double initialInvestment
        , double interestRate
        , int compoundingFrequency
        , int maturityMonths)
    {
        var j = maturityMonths * compoundingFrequency / 12.0;
        //if ((int)Math.Ceiling(j) != (int)Math.Floor(j))
        //{
        //    throw new InvalidOperationException(
        //        $"maturity month * compounding frequency / 12 should be integer," +
        //        $" but {maturityMonths} * {compoundingFrequency} / 12 = {j} and is not integer");
        //}

        var r = interestRate >= 1 ? interestRate/100.0 : interestRate;
        
        return initialInvestment * Math.Pow(1 + r / compoundingFrequency, j);
    }
}

