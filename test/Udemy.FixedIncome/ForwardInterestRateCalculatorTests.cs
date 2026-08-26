namespace Udemy.FixedIncome.Tests;

[Trait("Category", "Unit")]
public class ForwardInterestRateCalculatorTests(ITestOutputHelper output)
{

    [Fact]
    public void GetImpliedSpotCurve_Example2Lecture51_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1] = 0.006,
            [2] = 0.011,
            [3] = 0.014,
            [4] = 0.019,
            [5] = 0.023,
        };

        var curve = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 1);

        foreach (var (t,f) in curve)
        {
            output.WriteLine($"f (1, {t}) = {f:N4}");
        }
    }

    [Fact]
    public void GetOneYearForwardRates_Example3Lecture51_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1] = 0.008,
            [2] = 0.015,
            [3] = 0.019,
            [4] = 0.023,
            [5] = 0.025,
        };

        var curve = ForwardInterestRateCalculator.GetOneYearForwardRates(interestRates);

        foreach (var (t1, t2, f) in curve)
        {
            output.WriteLine($"f ({t1}, {t2}) = {f:N4}");
        }
    }
}