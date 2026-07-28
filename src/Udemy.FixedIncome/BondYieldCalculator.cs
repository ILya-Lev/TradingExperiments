using MathNet.Numerics.RootFinding;

namespace Udemy.FixedIncome;

public readonly record struct Bond(
    decimal MaturityYears,
    decimal FaceValue,
    decimal CouponPercentage,
    int CouponCompounding,
    decimal Yield,
    int YieldCompounding);

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

    public static double GetYieldAnnually(
        double dirtyPrice,
        double faceValue,
        double couponPercents,
        DateOnly maturity,
        DateOnly evaluationDate)
    {
        var couponPayment = faceValue * couponPercents;

        var nextPayment = GetNextPaymentAnnuallyDate(maturity, evaluationDate);
        var previousPayment = nextPayment.AddYears(-1);
        var tau = FractionUntilCouponDate(evaluationDate, nextPayment, previousPayment);

        var paymentsLeft = GetPaymentsNumberUntilMaturityAnnually(maturity, nextPayment);

        // 0 = A(y) - B
        Func<double, double> dirtyPriceEquation = y
            => Enumerable.Range(0, paymentsLeft + 1).Sum(shift => couponPayment / Math.Pow(1 + y, tau + shift))
               + faceValue / Math.Pow(1 + y, tau + paymentsLeft) 
               - dirtyPrice;

        var y = Secant.FindRoot(dirtyPriceEquation, 0.01, 0.05, 0.0, 0.9);
        return y;
    }

    public static double GetDirtyPriceAnnually(
        double faceValue,
        double couponPercents,
        double yieldPercents,
        DateOnly maturity,
        DateOnly evaluationDate)
    {
        var couponPayment = faceValue * couponPercents;

        var nextPayment = GetNextPaymentAnnuallyDate(maturity, evaluationDate);
        var previousPayment = nextPayment.AddYears(-1);
        var tau = FractionUntilCouponDate(evaluationDate, nextPayment, previousPayment);

        var paymentsLeft = GetPaymentsNumberUntilMaturityAnnually(maturity, nextPayment);
        var y = yieldPercents;

        var dirtyPrice = Enumerable.Range(0, paymentsLeft + 1).Sum(shift => couponPayment / Math.Pow(1 + y, tau + shift))
                         + faceValue / Math.Pow(1 + y, tau + paymentsLeft);

        return dirtyPrice;
    }

    public static double GetDirtyPriceSemiannually(
        double faceValue,
        double couponPercents,
        double yieldPercents,
        DateOnly maturity,
        DateOnly evaluationDate)
    {
        var couponPayment = faceValue * couponPercents / 2;

        var nextPayment = GetNextPaymentSemiannuallyDate(maturity, evaluationDate);
        var previousPayment = nextPayment.AddMonths(-6);
        var tau = FractionUntilCouponDate(evaluationDate, nextPayment, previousPayment);

        var paymentsLeft = GetPaymentsNumberUntilMaturitySemiannually(maturity, nextPayment);
        var y = yieldPercents / 2;

        var dirtyPrice = Enumerable.Range(0, paymentsLeft + 1).Sum(shift => couponPayment / Math.Pow(1 + y, tau + shift))
                         + faceValue / Math.Pow(1 + y, tau + paymentsLeft);

        return dirtyPrice;
    }

    public static double GetAccruedInterestAnnually(
        double faceValue,
        double couponPercents,
        DateOnly maturity,
        DateOnly evaluationDate)
    {
        var couponPayment = faceValue * couponPercents;

        var nextPayment = GetNextPaymentAnnuallyDate(maturity, evaluationDate);
        var previousPayment = nextPayment.AddYears(-1);
        var tau = FractionUntilCouponDate(evaluationDate, nextPayment, previousPayment);

        var accruedInterest = (1 - tau) * couponPayment;

        return accruedInterest;
    }

    public static double GetAccruedInterestSemiannually(
        double faceValue,
        double couponPercents,
        DateOnly maturity,
        DateOnly evaluationDate)
    {
        var couponPayment = faceValue * couponPercents / 2;

        var nextPayment = GetNextPaymentSemiannuallyDate(maturity, evaluationDate);
        var previousPayment = nextPayment.AddMonths(-6);
        var tau = FractionUntilCouponDate(evaluationDate, nextPayment, previousPayment);

        var accruedInterest = (1 - tau) * couponPayment;

        return accruedInterest;
    }

    private static double FractionUntilCouponDate(DateOnly evaluationDate, DateOnly next, DateOnly previous)
        => next.ToDateTime(TimeOnly.MinValue).Subtract(evaluationDate.ToDateTime(TimeOnly.MinValue)).TotalDays
           / next.ToDateTime(TimeOnly.MinValue).Subtract(previous.ToDateTime(TimeOnly.MinValue)).TotalDays;


    //a bit non-realistic as does not take into account holidays and weekends
    private static DateOnly GetNextPaymentAnnuallyDate(DateOnly maturity, DateOnly evaluationDate)
    {
        var current = maturity;

        while (current > evaluationDate)
            current = current.AddYears(-1);

        return current.AddYears(1);
    }

    private static DateOnly GetNextPaymentSemiannuallyDate(DateOnly maturity, DateOnly evaluationDate)
    {
        var current = maturity;

        while (current > evaluationDate)
            current = current.AddMonths(-6);

        return current.AddMonths(6);
    }

    private static int GetPaymentsNumberUntilMaturityAnnually(DateOnly maturity, DateOnly nextPayment)
    {
        var paymentsNumber = 0;

        while (nextPayment.AddYears(paymentsNumber) < maturity)
            paymentsNumber++;

        return paymentsNumber;
    }

    private static int GetPaymentsNumberUntilMaturitySemiannually(DateOnly maturity, DateOnly nextPayment)
    {
        var paymentsNumber = 0;

        while (nextPayment.AddMonths(6 * paymentsNumber) < maturity)
            paymentsNumber++;

        return paymentsNumber;
    }
}
