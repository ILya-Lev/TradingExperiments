using FluentAssertions;

namespace Udemy.FixedIncome.Tests;

[Trait("Category", "Unit")]
public class BondEstimatorsTests(ITestOutputHelper output)
{
    [Fact]//lecture 62, problem 1 a
    public void InterestRateShock_DollarDurationEstimation_DifferentMaturities_CompareWithPriceRecalculation()
    {
        var r = 4.0;
        var f = 500_000;
        var c = 3;
        var ts = new[] { 5, 10, 15, 20, 30 };

        foreach (var t in ts)
        {
            output.WriteLine($"\t\t\tmaturity T = {t} years:");
            var basePrice = BondEstimators.GetPriceFlatTerms(f, t, r, c);
            var dollarDuration = BondEstimators.GetDollarDurationFlatTerms(f, t, r, c);

            var rs = new[] { 1, 2, 3, 3.5, 4.5, 5, 6, 7 };

            foreach (var rate in rs)
            {
                var estimatedPriceChange = -(rate - r) / 100.0 * dollarDuration;
                var actualPriceChange = BondEstimators.GetPriceFlatTerms(f, t, rate, c) - basePrice;

                output.WriteLine($"dr = {rate - r} %, estimated change = {estimatedPriceChange:N4}, actual change = {actualPriceChange:N4}, error = {Math.Abs(estimatedPriceChange / actualPriceChange - 1):p2}");
            }
        }
    }

    [Fact]//lecture 62, problem 1 b
    public void InterestRateShock_DollarDurationEstimation_DifferentCoupons_CompareWithPriceRecalculation()
    {
        var r = 4.0;
        var f = 500_000;
        var cs = new[] { 3, 4, 5, 7, 10 };
        var t = 5;

        foreach (var c in cs)
        {
            output.WriteLine($"\t\t\tcoupon C = {c} %:");
            var basePrice = BondEstimators.GetPriceFlatTerms(f, t, r, c);
            var dollarDuration = BondEstimators.GetDollarDurationFlatTerms(f, t, r, c);

            var rs = new[] { 1, 2, 3, 3.5, 4.5, 5, 6, 7 };

            foreach (var rate in rs)
            {
                var estimatedPriceChange = -(rate - r) / 100.0 * dollarDuration;
                var actualPriceChange = BondEstimators.GetPriceFlatTerms(f, t, rate, c) - basePrice;

                output.WriteLine($"dr = {rate - r} %, estimated change = {estimatedPriceChange:N4}, actual change = {actualPriceChange:N4}, error = {Math.Abs(estimatedPriceChange / actualPriceChange - 1):p2}");
            }
        }
    }

    [Fact]//lecture 62, problem 1 c
    public void InterestRateShock_DollarDurationEstimation_DifferentInterestRates_CompareWithPriceRecalculation()
    {
        var rs = new[] { 3, 4, 5, 7, 10 };
        var f = 500_000;
        var c = 3;
        var t = 5;

        foreach (var r in rs)
        {
            output.WriteLine($"\t\t\tinterest rate r = {r} %:");
            var basePrice = BondEstimators.GetPriceFlatTerms(f, t, r, c);
            var dollarDuration = BondEstimators.GetDollarDurationFlatTerms(f, t, r, c);

            var drs = new[] { -3, -2, -1, -.5, .5, 1, 2, 3 };

            foreach (var dr in drs)
            {
                var estimatedPriceChange = -dr / 100.0 * dollarDuration;
                var actualPriceChange = BondEstimators.GetPriceFlatTerms(f, t, r + dr, c) - basePrice;

                output.WriteLine($"dr = {dr} %, estimated change = {estimatedPriceChange:N4}, actual change = {actualPriceChange:N4}, error = {Math.Abs(estimatedPriceChange / actualPriceChange - 1):p2}");
            }
        }
    }

    [Fact]//lecture 62, problem 2
    public void InterestRateShock_DollarDurationEstimation_CompareWithPriceRecalculation()
    {
        var r = 4;
        var portfolio = new[]
        {
            (T:5, F:100_000, C:0),
            (T:3, F:250_000, C:2),
            (T:7, F:500_000, C:2/*4.5*/),
            (T:15, F:450_000, C:5.5)
        };

        var price = portfolio.Sum(b => BondEstimators.GetPriceFlatTerms(b.F, b.T, r, b.C, 2));
        var duration = portfolio.Sum(b => BondEstimators.GetDollarDurationFlatTerms(b.F, b.T, r, b.C, 2));

        output.WriteLine($"portfolio price {price} and duration = {duration}");

        var drs = new[] { -4, -1, -.5, .5, 1, 4 };

        foreach (var dr in drs)
        {
            var estimatedPriceChange = -dr / 100.0 * duration;
            var changedPrice = portfolio.Sum(b => BondEstimators.GetPriceFlatTerms(b.F, b.T, r + dr, b.C, 2));
            var actualPriceChange = changedPrice - price;

            output.WriteLine($"dr = {dr} %; estimated {estimatedPriceChange:N4}; real {actualPriceChange:N4}; error = {Math.Abs(estimatedPriceChange / actualPriceChange - 1):N4}");
        }
    }


    [Fact]
    public void ReduceExposure_Long10YBond_Short2YBond()
    {
        var longPrice = BondEstimators.GetPriceFlatTerms(10_000_000, 10, 5, 4);
        var longDuration = BondEstimators.GetDollarDurationFlatTerms(10_000_000, 10, 5, 4);

        var shortDuration = BondEstimators.GetDollarDurationFlatTerms(100, 2, 5, 1.7);
        var hedgingRatio = -longDuration / shortDuration;

        var shortPrice = BondEstimators.GetPriceFlatTerms(100 * Math.Abs(hedgingRatio), 2, 5, 1.7);

        var initialPortfolioValue = longPrice - shortPrice;

        output.WriteLine($"long price = {longPrice:C2}, duration {longDuration}");
        output.WriteLine($"short price = {shortPrice:C2}, duration {shortDuration}");
        output.WriteLine($"hedging ratio {hedgingRatio}");
        output.WriteLine($"portfolio value {initialPortfolioValue:C2}");

        var drs = new[] { -3, -1, -0.1, 0.1, 1, 3 };
        foreach (var dr in drs)
        {
            var portfolioValue =
                BondEstimators.GetPriceFlatTerms(10_000_000, 10, 5 + dr, 4)
                - BondEstimators.GetPriceFlatTerms(100 * Math.Abs(hedgingRatio), 2, 5 + dr, 1.7);

            var valueChange = portfolioValue - initialPortfolioValue;

            output.WriteLine($"dr = {dr} %; portfolio value {portfolioValue:C2} and change {valueChange:C2}");
        }
    }

    [Fact]
    public void ReduceExposure_Long3YBond_Short10YBond()
    {
        var longPrice = BondEstimators.GetPriceFlatTerms(100_000, 3, 6, 7);
        var longDuration = BondEstimators.GetDollarDurationFlatTerms(100_000, 3, 6, 7);

        var shortDuration = BondEstimators.GetDollarDurationFlatTerms(100, 10, 6, 9);
        var hedgingRatio = -longDuration / shortDuration;

        var shortPrice = BondEstimators.GetPriceFlatTerms(100 * Math.Abs(hedgingRatio), 10, 6, 9);

        var initialPortfolioValue = longPrice - shortPrice;

        output.WriteLine($"long price = {longPrice:C2}, duration {longDuration}");
        output.WriteLine($"short price = {shortPrice:C2}, duration {shortDuration}");
        output.WriteLine($"hedging ratio {hedgingRatio}");
        output.WriteLine($"portfolio value {initialPortfolioValue:C2}");

        var drs = new[] { -3, -1, -0.1, 0.1, 1, 3 };
        foreach (var dr in drs)
        {
            var portfolioValue =
                BondEstimators.GetPriceFlatTerms(100_000, 3, 6 + dr, 7)
                - BondEstimators.GetPriceFlatTerms(100 * Math.Abs(hedgingRatio), 10, 6 + dr, 9);

            var valueChange = portfolioValue - initialPortfolioValue;

            output.WriteLine($"dr = {dr} %; portfolio value {portfolioValue:C2} and change {valueChange:C2}");
        }
    }

    [Fact]
    public void EstimatePriceChangesWithDuration_TreasuryStrip_ZeroCouponSemiannually()
    {
        var f = 200_000;
        var t = 10;
        var rate = 4.5;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var p = (double r) => f * Math.Pow(1 + r / 2.0, -2 * t);
        var d = (double r) => t / (1 + r / 2.0);

        var initialPrice = p(rate / 100);
        var duration = d(rate / 100);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = p(rate / 100 + dr);
            var realPriceChange = currentPrice - initialPrice;

            output.WriteLine($"dr {dr}; estimated {estimatedPriceChange:N4}; real {realPriceChange:N4}; price {currentPrice:N4}; estimation error {estimatedPriceChange / realPriceChange - 1:N4}");
            (estimatedPriceChange / realPriceChange).Should().BeApproximately(1.0, 0.22);
        }
    }

    [Fact]
    public void EstimatePriceChangesWithDuration_Gilt_UkNonzeroCouponSemiannually()
    {
        var f = 450_000;
        var t = 10;
        var c = 6 / 100.0;//6%
        var rate = 4.5;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var p = (double r) => f * (c / r + Math.Pow(1 + r / 2.0, -2 * t) * (1 - c / r));
        var d = (double r) => f / p(r) * (Math.Pow(1 + r / 2.0, -2 * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / 2.0, -2 * t)));

        var initialPrice = p(rate / 100);
        var duration = d(rate / 100);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = p(rate / 100 + dr);
            var realPriceChange = currentPrice - initialPrice;

            output.WriteLine($"dr {dr}; estimated {estimatedPriceChange:N4}; real {realPriceChange:N4}; price {currentPrice:N4}; estimation error {estimatedPriceChange / realPriceChange - 1:N4}");
            (estimatedPriceChange / realPriceChange).Should().BeApproximately(1.0, 0.20);
        }
    }

    [Fact]
    public void EstimatePriceChangesWithDuration_Bund_GeNonzeroCouponAnnually()
    {
        var f = 600_000;
        var t = 20;
        var c = 2 / 100.0;//2%
        var rate = 4.5;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var p = (double r) => f * (c / r + Math.Pow(1 + r, -t) * (1 - c / r));
        var d = (double r) => f / p(r) * (Math.Pow(1 + r, -t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r, -t)));

        var initialPrice = p(rate / 100);
        var duration = d(rate / 100);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = p(rate / 100 + dr);
            var realPriceChange = currentPrice - initialPrice;

            output.WriteLine($"dr {dr}; estimated {estimatedPriceChange:N4}; real {realPriceChange:N4}; price {currentPrice:N4}; estimation error {estimatedPriceChange / realPriceChange - 1:N4}");
            (estimatedPriceChange / realPriceChange).Should().BeApproximately(1.0, 0.41);
        }
    }

    [Fact]
    public void EstimatePriceChangesWithDuration_TreasuryBond_UsaNonzeroCouponSemiannually()
    {
        var f = 1_500_000;
        var t = 30;
        var c = 5.5 / 100.0;//5.5%
        var rate = 4.5;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var p = (double r) => f * (c / r + Math.Pow(1 + r / 2.0, -2 * t) * (1 - c / r));
        var d = (double r) => f / p(r) * (Math.Pow(1 + r / 2.0, -2 * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / 2.0, -2 * t)));

        var initialPrice = p(rate / 100);
        var duration = d(rate / 100);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = p(rate / 100 + dr);
            var realPriceChange = currentPrice - initialPrice;

            output.WriteLine($"dr {dr}; estimated {estimatedPriceChange:N4}; real {realPriceChange:N4}; price {currentPrice:N4}; estimation error {estimatedPriceChange / realPriceChange - 1:N4}");
            (estimatedPriceChange / realPriceChange).Should().BeApproximately(1.0, 0.51);
        }
    }

    [Fact]
    public void CalculateDuration_ForInterestRateChanges_PlotIt()
    {
        var f = 100_000;
        var t = 5;
        var c = 3 / 100.0;//3%
        var k = 1;
        var rates = new[] { 1, 2, 3, 5, 7, 10, 15, 20 };

        var p = (double r) => f * (c / r + Math.Pow(1 + r / k, -k * t) * (1 - c / r));
        var d = (double r) => f / p(r) * (Math.Pow(1 + r / k, -k * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / k, -k * t)));

        foreach (var r in rates.Select(r => r / 100.0))
        {
            var duration = d(r);
            output.WriteLine($"r {r:P}; duration {duration:N4}");
            duration.Should().BeLessThan(t);
        }
    }

    [Fact]
    public void CalculateDuration_ForCouponChanges_PlotIt()
    {
        var f = 100_000;
        var t = 5;
        var coupons = new[] { 1, 2, 3, 4, 5, 7, 10, 12, 15, 20 };
        var k = 1;
        var r = 5 / 100.0;//5%

        var p = (double c) => f * (c / r + Math.Pow(1 + r / k, -k * t) * (1 - c / r));
        var d = (double c) => f / p(c) * (Math.Pow(1 + r / k, -k * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / k, -k * t)));

        foreach (var c in coupons.Select(c => c / 100.0))
        {
            var duration = d(c);
            output.WriteLine($"c {c:P}; duration {duration:N4}");
            duration.Should().BeLessThan(t);
        }
    }

    [Fact]
    public void CalculateDuration_ForMaturityChanges_PlotIt()
    {
        var f = 100_000;
        var maturities = new[] { 5, 10, 15, 20, 25, 30 };
        var c = 5 / 100.0;//3%
        var k = 1;
        var rates = new[] { 2, 5, 15 };

        var p = (double r, int t) => f * (c / r + Math.Pow(1 + r / k, -k * t) * (1 - c / r));
        var d = (double r, int t) => f / p(r, t) * (Math.Pow(1 + r / k, -k * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / k, -k * t)));

        foreach (var r in rates.Select(r => r / 100.0))
        {
            output.WriteLine($"interest rate = {r:P}");
            foreach (var t in maturities)
            {
                var duration = d(r, t);
                output.WriteLine($"\t\tt {t:D2}; duration {duration:N4}");
                duration.Should().BeLessThan(t);
            }
        }
    }

    [Fact]
    public void EstimatePortfolioValueChangesWithDuration_ForInterestRateChanges_CompareWithReevaluation()
    {
        var portfolio = new[]
        {
            (T: 10, F: 140_000, C: 0.0),
            (T: 05, F: 350_000, C: 1.5),
            (T: 07, F: 270_000, C: 3.0),
            (T: 15, F: 650_000, C: 7.0),
            (T: 25, F: 530_000, C: 4.5),
        };
        
        var k = 2;
        var rate = 3.5 / 100.0;
        var drs = new[] { 10, 25, 100, 300 }.SelectMany(dr => new[] { -dr, dr }).OrderBy(dr => dr).ToArray();

        var price = (double r, int t, int f, double c) => f * (c / r + Math.Pow(1 + r / k, -k * t) * (1 - c / r));
        var dollarDuration = (double r, int t, int f, double c) => f * (Math.Pow(1 + r / k, -k * t - 1) * t * (1 - c / r) + c / r / r * (1 - Math.Pow(1 + r / k, -k * t)));

        var portfolioValue = (double r) => portfolio.Sum(bond => price(r, bond.T, bond.F, bond.C));

        var initialPortfolioValue = portfolioValue(rate);
        var duration = portfolio.Sum(bond => dollarDuration(rate, bond.T, bond.F, bond.C)) / portfolioValue(rate);

        output.WriteLine($"initial portfolio value {initialPortfolioValue:N4}; duration {duration:N4}");

        foreach (var dr in drs.Select(dr => dr / 10_000.0))
        {
            var estimatedPortfolioValueChange = -duration * initialPortfolioValue * dr;
            var currentPortfolioValue = portfolioValue(rate + dr);
            var realPortfolioValueChange = currentPortfolioValue - initialPortfolioValue;
            
            output.WriteLine($"dr {dr:P4}; portfolio value {currentPortfolioValue:N4};" +
                             $" estimated change {estimatedPortfolioValueChange:N4};" +
                             $" real change {realPortfolioValueChange:N4};" +
                             $" estimation error {estimatedPortfolioValueChange/realPortfolioValueChange - 1:N4}");
            
            Math.Abs(estimatedPortfolioValueChange / realPortfolioValueChange - 1).Should().BeLessThan(.20);
        }
    }
}