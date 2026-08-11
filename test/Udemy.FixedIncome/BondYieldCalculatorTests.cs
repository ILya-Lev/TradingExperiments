using FluentAssertions;
using FluentAssertions.Execution;
using ScottPlot;

namespace Udemy.FixedIncome.Tests;

public class BondYieldCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public void GetCashFlow_4Y10k6pAnnually_Observe()
    {
        var bond = new Bond(4, 10_000, 
            0.06m, 1, 
            0, 1);
        
        var cashFlow = bond.GetCashFlow().ToArray();
        
        cashFlow.Should().BeEqualTo([(1, 600), (2, 600), (3, 600), (4, 10_600)]);
    }

    [Fact]
    public void GetPrice_4Y10k6pAnnually_Observe()
    {
        var bond = new Bond(4, 10_000, 
            0.06m, 1, 
            0, 1);
        var bonds = new[]{4, 5, 6, 7, 8}.Select(y => bond with { Yield = y / 100m }).ToArray();
        
        var prices = bonds.Select(b => b.GetPrice()).ToArray();

        prices.Should().BeEquivalentTo([10725.979M, 10354.595M, 10_000, 9661.2788M, 9337.5746M],
        options => options
            .WithStrictOrdering()
            .Using<decimal>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1e-4m))
            .WhenTypeIs<decimal>());
    }

    [Fact]
    public void GetPrice_2Y10k5pAnnually7pYield_DifferentCompounding()
    {
        var annualBond = new Bond(2, 10_000, 
            0.05m, 1, 
            0.07m, 1);
        var bonds = new[] { 1, 2, 4 }.Select(k => annualBond with { YieldCompounding = k }).ToArray();

        var prices = bonds.Select(b => b.GetPrice()).ToArray();

        prices.Should().BeEquivalentTo([9638.3964M, 9616.8987M, 9605.8007M],
        options => options
            .WithStrictOrdering()
            .Using<decimal>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1e-4m))
            .WhenTypeIs<decimal>());
    }

    [Fact]
    public void GetPrice_2Y25k4p_DifferentCompounding_CommensuratingYields()
    {
        var annualBond = new Bond(2, 25_000, 
            0.04m, 1, 
            0.02m, 1);
        var compoundings = new[] { 1, 2, 4 };
        var yields = new[] { 2, 4, 6, }.Select(y => y / 100m);

        var bonds = compoundings.SelectMany(k => yields.Select(y => annualBond with
        {
            YieldCompounding = k,
            CouponCompounding = k,  //as commensurating (matching) coupon and yield compounding by the task description
            Yield = y
        })).ToArray();

        var prices = bonds.Select(b => b.GetPrice()).ToArray();

        //when we have both coupon and yield percentage and compounding matching, price coincides with face value
        prices.Should().BeEquivalentTo(
            [
                25970.7804m, 25000m, 24083.3036m, 25975.4913m, 25000m, 24070.7253m, 25977.8699m, 25000m, 24064.2593m
            ],
        options => options
            .WithStrictOrdering()
            .Using<decimal>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1e-4m))
            .WhenTypeIs<decimal>());
    }

    [Fact]
    public void GetPrice_2Y20k7cp4yp_SemiannuallyBoth_Observe()
    {
        var semiannualBond = new Bond(2, 20_000, 
            0.07m, 2,
            0.04m, 2);

        var price = semiannualBond.GetPrice();

        price.Should().BeApproximately(21142.32m, 1e-2m);
    }

    [Fact]
    public void GetPrice_3Y20k2cp45yp_SemiannuallyBoth_Observe()
    {
        var semiannualBond = new Bond(3, 20_000, 
            0.02m, 2,
            0.045m, 2);

        var price = semiannualBond.GetPrice();

        price.Should().BeApproximately(18611.38m, 1e-2m);
    }

    [Fact]
    public void GetPrice_2Y25k4cp_YieldRange_MatchingCompounding_Observe()
    {
        var protoBond = new Bond(2, 25_000, 
            0.04m, 2,
            0.045m, 2);

        var compoundings = new[] { 1, 2, 4 };
        var yields = new[] { 2, 4, 6 }.Select(y => y / 100m);

        var prices = compoundings.SelectMany(k => yields.Select(y => protoBond with
            {
                CouponCompounding = k,
                YieldCompounding = k,
                Yield = y
            }))
            .Select(b => b.GetPrice())
            .ToArray();

        //when we have both coupon and yield percentage and compounding matching, price coincides with face value
        prices.Should().BeEquivalentTo(
            [
                25970.7804m, 25000m, 24083.3036m, 25975.4913m, 25000m, 24070.7253m, 25977.8699m, 25000m, 24064.2593m
            ],
            options => options
                .WithStrictOrdering()
                .Using<decimal>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1e-4m))
                .WhenTypeIs<decimal>());
    }

    [Fact]
    public void GetPrice_3Y100nv4pAnnually_PlotRangeOfYields()
    {
        var annualBond = new Bond(3, 100, 
            0.04m, 1,
            0.01m, 1);
        var coupon = new[] { 2, 4, 6 }.Select(c => c / 100m);
        var yields = Enumerable.Range(1, 10).Select(y => y / 100m);

        var bonds = coupon.ToDictionary(c => c, c => yields.Select(y => annualBond with
        {
            CouponPercentage = c,
            Yield = y
        }).ToArray());

        foreach (var (couponPercentage, bondsPerYield) in bonds)
        {
            var pricePerYield = bondsPerYield.Select(b => (b.Yield, b.GetPrice())).ToArray();
            var info = PlotSeries($"bond per yield for {couponPercentage}% coupon", pricePerYield, false);
            output.WriteLine(info.Path);
        }
    }

    [Fact]
    public void GetYieldAnnually_GovBondsP1_Observe()
    {
        var faceValue = 500_000;
        var dirtyPrice = 472_657.5;
        var couponPercents = 0.02;
        var maturity = new DateOnly(2022, 03, 15);
        var evaluationDate = new DateOnly(2018, 06, 20);

        var y = BondYieldCalculator.GetYieldAnnually(dirtyPrice, faceValue, couponPercents, maturity, evaluationDate);
        y.Should().BeApproximately(0.03749, 1e-5);

        var calculatedDirtyPrice = BondYieldCalculator.GetDirtyPriceAnnually(faceValue, couponPercents, y, maturity, evaluationDate);
        calculatedDirtyPrice.Should().BeApproximately(dirtyPrice, 1e-1);
    }

    [Fact]
    public void GetDirtyPriceSemiannually_GovBondsP2_Observe()
    {
        var faceValue = 20_000;
        var couponPercents = 0.05;
        var yieldPercents = 0.035;
        var maturity = new DateOnly(2021, 07, 15);
        var evaluationDate = new DateOnly(2020, 09, 25);

        var dirtyPrice = BondYieldCalculator.GetDirtyPriceSemiannually(faceValue, couponPercents, yieldPercents, maturity, evaluationDate);
        var accruedInterest = BondYieldCalculator.GetAccruedInterestSemiannually(faceValue, couponPercents, maturity, evaluationDate);
        var cleanPrice = dirtyPrice - accruedInterest;
        
        using var _ = new AssertionScope();
        dirtyPrice.Should().BeApproximately(20_430.53, 1e-2);
        cleanPrice.Should().BeApproximately(20_234.88, 1e-2);
        accruedInterest.Should().BeApproximately(195.65, 1e-2);
    }

    [Fact]
    public void GetDiscountMargin_FloatingRateNote_Example1()
    {
        var sofr = 0.03_50;
        var spread = 0.00_50;
        var c = sofr + spread;

        var y = BondYieldCalculator.GetYieldAnnually(96, 100, c, 3);
        var d = y - sofr;

        d.Should().BeApproximately(0.01_982, 1e-4);
    }

    [Fact]
    public void GetPrice_CollaredSemiannuallyFloatingRateNote_Example2()
    {
        var rs = new[] { 3.8, 3.6, 3.0, 2.4 };
        var cap = 4;
        var floor = 2.5;
        var spread = 0.5;

        var collaredInterestRates = rs.Select(r => r + spread)
            .Select(r => Math.Min(cap, r))
            .Select(r => Math.Max(floor, r))
            .Select(r => r/200.0)//as semiannually and to convert from % to decimals
            .ToArray();

        var p = collaredInterestRates.Select((c, i) => c * Math.Pow(1 + rs[i]/200.0, -(i+1))).Sum()
            + Math.Pow(1 + rs.Last() / 200.0, -rs.Length)
            ;

        p.Should().BeApproximately(1.02_29, 1e-4);
    }

    public static SavedImageInfo PlotSeries(string sourceName
        , IReadOnlyCollection<(decimal x, decimal y)> series
        , bool addDiagonal = true)
    {
        var chartName = $"{sourceName}.svg";
        var chartPath = Path.Combine(Directory.GetCurrentDirectory(), "charts", chartName);
        Directory.CreateDirectory(Path.GetDirectoryName(chartPath) ?? throw new InvalidOperationException());

        var plot = new Plot();
        plot.Title(sourceName);
        var palette = new ScottPlot.Palettes.Category10();

        var line = plot.Add.Scatter
        (
            series.Select(p => new Coordinates((double)p.x, (double)p.y)).ToArray()
        );
        line.Color = palette.GetColor(0);
        line.LineWidth = 2;

        if (addDiagonal)
        {
            var from = Math.Min(series.First().x, series.First().y);
            var to = Math.Max(series.Last().x, series.Last().y);
            var diagonalPoints = new List<Coordinates>();
            for (var x = from; x <= to; x += 0.01m)
            {
                diagonalPoints.Add(new Coordinates((double)x, (double)x));
            }
            var mainDiagonal = plot.Add.ScatterLine(diagonalPoints);
            mainDiagonal.Color = palette.GetColor(4);
            mainDiagonal.LineWidth = 2;
        }

        plot.Axes.Right.IsVisible = true;
        plot.Axes.Top.IsVisible = true;

        plot.RenderManager.RenderStarting += (sender, args) =>
        {
            plot.Axes.Right.Min = plot.Axes.Left.Min;
            plot.Axes.Right.Max = plot.Axes.Left.Max;
            plot.Axes.Top.Min = plot.Axes.Bottom.Min;
            plot.Axes.Top.Max = plot.Axes.Bottom.Max;
        };

        return plot.SaveSvg(chartPath, 1980, 1020);
    }


}
