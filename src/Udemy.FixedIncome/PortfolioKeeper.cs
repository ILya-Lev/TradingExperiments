namespace Udemy.FixedIncome;

public readonly record struct Investment(double Amount, DateOnly From, DateOnly To, double InterestRate, int CompoundingFrequency);
public class PortfolioKeeper
{
    private readonly List<Investment> _investments = new();
    private double _savings = 0;
    private DateOnly _lastEvaluationDate = DateOnly.MinValue;

    public void AddInvestment(Investment i) => _investments.Add(i);

    public void Evaluate(DateOnly date)
    {
        var newSavings = ReviewInvestmetns(date);
    }

    private double ReviewInvestmetns(DateOnly date)
    {
        var newSavings = 0.0;

        var investmentsUnderExamination = _investments.ToArray();
        _investments.Clear();

        foreach (var investment in investmentsUnderExamination)
        {
            if (investment.To > date)
            {
                _investments.Add(investment);
                continue;
            }

            var compoundedAmount = GetCompoundedAmount(investment);
            newSavings += compoundedAmount;
        }

        return newSavings;
    }

    private static double GetCompoundedAmount(Investment investment)
    {
        var foundation = 1 + investment.InterestRate / investment.CompoundingFrequency;
        var power = DaysBetween(investment.From, investment.To) / 365 * investment.CompoundingFrequency;
        
        var compoundedAmount = investment.Amount
            * Math.Abs(power - 1.0) < 1e-2
                ? foundation
                : Math.Pow(foundation, power);
        
        return compoundedAmount;
    }

    public static int DaysBetween(DateOnly from, DateOnly to)
    {
        var daysBetween = 0;
        for (DateOnly current = from; current < to; current = current.AddDays(1))
        {
            daysBetween++;
        }
        return daysBetween;
    }
}