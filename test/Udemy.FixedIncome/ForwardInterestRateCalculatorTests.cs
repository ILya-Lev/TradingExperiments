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

        var curve = ForwardInterestRateCalculator.GetForwardRatesShifted(interestRates);

        foreach (var (t1, t2, f) in curve)
        {
            output.WriteLine($"f ({t1}, {t2}) = {f:N4}");
        }
    }

    [Fact]
    public void GetForwardRates_Task1Lecture51_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1] = 0.012,
            [2] = 0.018,
            [3] = 0.023,
            [4] = 0.025,
            [5] = 0.026,
        };

        var curve1 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 1);
        var curve3 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 3);
        var curve = ForwardInterestRateCalculator.GetForwardRatesShifted(interestRates);

        foreach (var (t, f) in curve1)
        {
            output.WriteLine($"f (1, {t}) = {f:N4}");
        }

        foreach (var (t, f) in curve3)
        {
            output.WriteLine($"f (3, {t}) = {f:N4}");
        }

        foreach (var (t1, t2, f) in curve)
        {
            output.WriteLine($"f ({t1}, {t2}) = {f:N4}");
        }
    }

    [Fact]
    public void GetForwardRates_Task3Lecture51_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [0.5] = 0.015,
            [1.0] = 0.024,
            [1.5] = 0.031,
            [2.0] = 0.037,
            [2.5] = 0.040,
            [3.0] = 0.042,
        };

        var curve05 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 0.5, 2);

        foreach (var (t, f) in curve05)
        {
            output.WriteLine($"f (0.5, {t:N1}) = {f:N4}");
        }
    }

    [Fact]
    public void GetForwardRates_Task2Lecture52_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [0.5] = 0.0028,
            [1.0] = 0.0092,
            [1.5] = 0.0145,
            [2.0] = 0.0195,
            [3.0] = 0.0250,
            [4.0] = 0.0316,
            [5.0] = 0.0351,
            [6.0] = 0.0393,
            [8.0] = 0.0415,
            [10.0] = 0.0428,
        };

        var interpolated = interestRates.Select(p => (p.Key, p.Value)).Interpolate(0.5).ToDictionary();

        var curve1 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interpolated, 1);
        var curve1Semi = ForwardInterestRateCalculator.GetImpliedSpotCurve(interpolated, 1, 2);
        var curve5 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interpolated, 5);
        var curve5Semi = ForwardInterestRateCalculator.GetImpliedSpotCurve(interpolated, 5, 2);

        foreach (var (t, f) in curve1) output.WriteLine($"f (1, {t:N1}) = {f:N4}");
        foreach (var (t, f) in curve1Semi) output.WriteLine($"f (1, {t:N1}) = {f:N4}");
        foreach (var (t, f) in curve5) output.WriteLine($"f (5, {t:N1}) = {f:N4}");
        foreach (var (t, f) in curve5Semi) output.WriteLine($"f (5, {t:N1}) = {f:N4}");
    }
}