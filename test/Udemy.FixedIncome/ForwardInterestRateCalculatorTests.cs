using FluentAssertions;
using FluentAssertions.Execution;

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

    [Fact]//lecture 55 problem 2 
    public void CalculatePnL_RealizedInterestRates_ImpliedInterestRates()
    {
        var interestRates = new Dictionary<double, double>()
        {
            [1] = 1.05,
            [2] = 1.23,
            [3] = 1.42,
            [4] = 1.57,
            [5] = 1.72,
            [6] = 1.85,
            [7] = 1.93,
            [8] = 2.02,
            [9] = 2.11,
            [10] = 2.14,
        };

        var highForecastInterestRates = new Dictionary<double, double>() //r (1,T), where T is the key in the dictionary
        {
            [2] = 1.55,
            [3] = 1.83,
            [4] = 1.92,
            [5] = 1.99,
            [6] = 2.05,
            [7] = 2.11,
            [8] = 2.15,
            [9] = 2.19,
            [10] = 2.23,
        };

        var lowForecastInterestRates = new Dictionary<double, double>() //r (1,T), where T is the key in the dictionary
        {
            [2] = 1.25,
            [3] = 1.38,
            [4] = 1.51,
            [5] = 1.64,
            [6] = 1.73,
            [7] = 1.81,
            [8] = 1.89,
            [9] = 1.97,
            [10] = 2.03,
        };

        var impliedInterestRates = interestRates.Skip(1)    //f (1, T), where T is the key in the dictionary
            .ToDictionary(p => p.Key, p => 100*(Math.Pow(Math.Pow(1 + p.Value / 100, p.Key) / (1 + interestRates[1] / 100), 1.0/(p.Key-1)) - 1));

        output.WriteLine($"T years: \t{string.Join("\t\t", interestRates.Keys.Skip(1))}");
        output.WriteLine($"r(T), %: \t{string.Join("\t", interestRates.Values.Skip(1))}");
        output.WriteLine($"rh(1, T), %: {string.Join("\t", highForecastInterestRates.Values)}");
        output.WriteLine($"rl(1, T), %: {string.Join("\t", lowForecastInterestRates.Values)}");
        output.WriteLine($"f(1, T), %: {string.Join("\t", impliedInterestRates.Values.Select(f => f.ToString("N2")))}");


        var portfolio = new Dictionary<double, double>()//of zero coupon bonds with maturity = key, face value = value of the dictionary
        {
            [3] = 270_000,
            [5] = 150_000,
            [7] = 270_000,
        };

        var assets0 = portfolio.Sum(p => p.Value * Math.Pow(1 + interestRates[p.Key] / 100, -p.Key));
        var liabilities0 = assets0;//as we borrow tehse funds to purchase these bonds today.
        var portfolioValue0 = assets0 - liabilities0;//is 0

        var assets1High = portfolio.Sum(p => p.Value * Math.Pow(1 + highForecastInterestRates[p.Key] / 100, -(p.Key-1)));
        var liabilities1 = liabilities0 * (1 + interestRates[1]/100);//as we borrow for 1 year, annually compounded
        var portfolioValue1High = assets1High - liabilities1;//expected to be < 0 as interest rates went up and bond prices dropped => out portfolio value dropped
        
        var assets1Low = portfolio.Sum(p => p.Value * Math.Pow(1 + lowForecastInterestRates[p.Key] / 100, -(p.Key-1)));
        var portfolioValue1Low = assets1Low - liabilities1;//expected to be > 0 as interest rates went down and bond prices increased

        output.WriteLine($"assets0 = {assets0}");
        output.WriteLine($"portfolio value 1 high = {assets1High} - {liabilities1} = {portfolioValue1High}");
        output.WriteLine($"portfolio value 1 low  = {assets1Low} - {liabilities1} = {portfolioValue1Low}");

        using (var _ = new AssertionScope())
        {
            portfolioValue0.Should().BeApproximately(0, 1e-6);
            portfolioValue1High.Should().BeApproximately(-2_163, 1);
            portfolioValue1Low.Should().BeApproximately(6_315, 1);
        }
        //-------------------------------------------------------------------------------------------
        //adding 5 year coupon bond paying annual coupons of 3.5% with 450k face value
        //-------------------------------------------------------------------------------------------

        //bond price = sum of present values of coupon payments + present value of face value
        var bond0 = GetCouponBondPrice(5, interestRates);
        var bond1High = GetCouponBondPrice(4, highForecastInterestRates, 1);
        var bond1Low = GetCouponBondPrice(4, lowForecastInterestRates, 1); 
        
        var assets0WithCoupon = assets0 + bond0;
        var liabilities0WithCoupon = assets0WithCoupon;
        var portfolioValue0WithCoupon = assets0WithCoupon - liabilities0WithCoupon;

        var assets1WithCouponHigh = assets1High + bond1High + 0.035 * 450_000;//include coupon into the portfolio as an asset
        var liabilities1WithCoupon = liabilities0WithCoupon * (1 + interestRates[1] / 100);
        var portfolioValue1WithCouponHigh = assets1WithCouponHigh - liabilities1WithCoupon;
        
        var assets1WithCouponLow = assets1Low + bond1Low + 0.035 * 450_000;//include coupon into the portfolio as an asset
        var portfolioValue1WithCouponLow = assets1WithCouponLow - liabilities1WithCoupon;

        output.WriteLine($"assets0WithCoupon = {assets0WithCoupon}");
        output.WriteLine($"portfolio value 1 high WithCoupon = {assets1WithCouponHigh} - {liabilities1WithCoupon} = {portfolioValue1WithCouponHigh}");
        output.WriteLine($"portfolio value 1 low  WithCoupon = {assets1WithCouponLow} - {liabilities1WithCoupon} = {portfolioValue1WithCouponLow}");

        using (var _ = new AssertionScope())
        {
            portfolioValue0WithCoupon.Should().BeApproximately(0, 1e-6);
            portfolioValue1WithCouponHigh.Should().BeApproximately(-2_908, 1);
            portfolioValue1WithCouponLow.Should().BeApproximately(12_912, 1);
        }


        static double GetCouponBondPrice(int yearsToMaturity, Dictionary<double, double> rates, int shift = 0) =>
            450_000 * (0.035 * Enumerable.Range(1, yearsToMaturity).Sum(t => Math.Pow(1 + rates[t+shift] / 100, -t))
                       + Math.Pow(1 + rates[yearsToMaturity] / 100, -yearsToMaturity));
    }
}