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
                var actualPriceChange = BondEstimators.GetPriceFlatTerms(f, t, r+dr, c) - basePrice;

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
            var estimatedPriceChange = -dr/100.0 * duration;
            var changedPrice = portfolio.Sum(b => BondEstimators.GetPriceFlatTerms(b.F, b.T, r + dr, b.C, 2));
            var actualPriceChange = changedPrice - price;

            output.WriteLine($"dr = {dr} %; estimated {estimatedPriceChange:N4}; real {actualPriceChange:N4}; error = {Math.Abs(estimatedPriceChange/actualPriceChange - 1):N4}");
        }
    }


    [Fact]
    public void ReduceExposure_Long10YBond_Short2YBond()
    {
        var longPrice = BondEstimators.GetPriceFlatTerms(10_000_000, 10, 5, 4);
        var longDuration = BondEstimators.GetDollarDurationFlatTerms(10_000_000, 10, 5, 4);

        var shortDuration = BondEstimators.GetDollarDurationFlatTerms(100, 2, 5, 1.7);
        var hedgingRatio = - longDuration/ shortDuration;

        var shortPrice = BondEstimators.GetPriceFlatTerms(100*Math.Abs(hedgingRatio), 2, 5, 1.7);

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
                - BondEstimators.GetPriceFlatTerms(100 * Math.Abs(hedgingRatio), 2, 5+dr, 1.7);

            var valueChange = portfolioValue - initialPortfolioValue;

            output.WriteLine($"dr = {dr} %; portfolio value {portfolioValue:C2} and change {valueChange:C2}");
        }
    }
}