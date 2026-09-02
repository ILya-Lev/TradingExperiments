using FluentAssertions;

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
    
    [Fact]
    public void GetForwardRates_Task3Lecture53_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1.0] = 0.0087,
            [2.0] = 0.0153,
            [3.0] = 0.0214,
            [4.0] = 0.0253,
            [5.0] = 0.0287,
            [6.0] = 0.0314,
        };

        var curve3 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 3).Prepend(new KeyValuePair<double, double>(3,0)).ToDictionary();
        output.WriteLine($"implied forward rates in 3 years: ");
        foreach (var (t, f) in curve3) output.WriteLine($"f (3, {t:N1}) = {f:N4}");

        var bondPrices = interestRates.Where(p => p.Key >= 3).Select(p => Math.Pow(1 + p.Value, -p.Key)).ToArray();
        var heldPrices = curve3.Select(p => Math.Pow(1 + p.Value, -p.Key+3)).ToArray();

        var holdingPeriodReturn = bondPrices.Zip(heldPrices, (b, h) => Math.Cbrt(h / b) - 1).ToArray();
        output.WriteLine($"holding for 3 years return: ");
        foreach (var r in holdingPeriodReturn) output.WriteLine($"return is {r:N4}");

        holdingPeriodReturn.Should().OnlyContain(h3 => Math.Abs(h3 - interestRates[3]) < 1e-9);
    }

    [Fact]
    public void GetForwardRates_Task4Lecture53_Observe()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1.0] = 0.0043,
            [2.0] = 0.0091,
            [3.0] = 0.0148,
            [4.0] = 0.0191,
            [5.0] = 0.0239,
            [6.0] = 0.0257,
            [7.0] = 0.0289,
            [8.0] = 0.0312,
            [9.0] = 0.0328,
            [10.0] = 0.0351,
        };

        var curve2 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 2);
        var curve3 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 3);
        var curve6 = ForwardInterestRateCalculator.GetImpliedSpotCurve(interestRates, 6);
        output.WriteLine($"implied forward rates in 2 years: ");
        foreach (var (t, f) in curve2) output.WriteLine($"f (2, {t:N1}) = {f:N4}");
        output.WriteLine($"implied forward rates in 2 years: ");
        foreach (var (t, f) in curve3) output.WriteLine($"f (3, {t:N1}) = {f:N4}");
        output.WriteLine($"implied forward rates in 6 years: ");
        foreach (var (t, f) in curve6) output.WriteLine($"f (6, {t:N1}) = {f:N4}");

        var bondPrices = interestRates.ToDictionary(p => p.Key, p => Math.Pow(1 + p.Value, -p.Key));
        
        var heldPrices2 = curve2.Select(p => Math.Pow(1 + p.Value, -p.Key+2)).ToArray();
        var heldPrices3 = curve3.Select(p => Math.Pow(1 + p.Value, -p.Key+3)).ToArray();
        var heldPrices6 = curve6.Select(p => Math.Pow(1 + p.Value, -p.Key+6)).ToArray();

        var return2 = bondPrices.Skip(2).Zip(heldPrices2, (b, h) => Math.Sqrt(h / b.Value) - 1).ToArray();
        output.WriteLine($"holding for 2 years return: ");
        foreach (var r in return2) output.WriteLine($"return is {r:N4}");

        var return3 = bondPrices.Skip(3).Zip(heldPrices3, (b, h) => Math.Cbrt(h / b.Value) - 1).ToArray();
        output.WriteLine($"holding for 3 years return: ");
        foreach (var r in return3) output.WriteLine($"return is {r:N4}");
        
        var return6 = bondPrices.Skip(6).Zip(heldPrices6, (b, h) => Math.Pow(h / b.Value, 1.0/6) - 1).ToArray();
        output.WriteLine($"holding for 6 years return: ");
        foreach (var r in return6) output.WriteLine($"return is {r:N4}");


        return2.Should().OnlyContain(h2 => Math.Abs(h2 - interestRates[2]) < 1e-9);
        return3.Should().OnlyContain(h3 => Math.Abs(h3 - interestRates[3]) < 1e-9);
        return6.Should().OnlyContain(h6 => Math.Abs(h6 - interestRates[6]) < 1e-9);
    }
}