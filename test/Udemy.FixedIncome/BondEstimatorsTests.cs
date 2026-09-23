using FluentAssertions;
using static Udemy.FixedIncome.BondMetricsCalculator;

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

        var p = (double r) => GetBondPrice(r, t, f, 0, 2);
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
        var rate = 4.5 / 100.0;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var initialPrice = GetBondPrice(rate, t, f, c, 2);
        var duration = GetBondDuration(rate, t, f, c, 2);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = GetBondPrice(rate + dr, t, f, c, 2);
            var realPriceChange = currentPrice - initialPrice;

            output.WriteLine($"dr {dr:P}; estimated {estimatedPriceChange:N4}; real {realPriceChange:N4}; price {currentPrice:N4}; estimation error {estimatedPriceChange / realPriceChange - 1:N4}");
            (estimatedPriceChange / realPriceChange).Should().BeApproximately(1.0, 0.20);
        }
    }

    [Fact]
    public void EstimatePriceChangesWithDuration_Bund_GeNonzeroCouponAnnually()
    {
        var f = 600_000;
        var t = 20;
        var c = 2 / 100.0;//2%
        var rate = 4.5 / 100.0;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var initialPrice = GetBondPrice(rate, t, f, c);
        var duration = GetBondDuration(rate, t, f, c);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = GetBondPrice(rate + dr, t, f, c);
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
        var rate = 4.5 / 100.0;//4.5% flat level
        var drs = new[] { 50, 100, 200, 400 }.SelectMany(dr => new[] { -1 * dr, dr }).OrderBy(dr => dr).ToArray();

        var initialPrice = GetBondPrice(rate, t, f, c, 2);
        var duration = GetBondDuration(rate, t, f, c, 2);

        output.WriteLine($"price {initialPrice} and duration {duration}");

        foreach (var dr in drs.Select(e => e * 1.0 / 10_000))
        {
            var estimatedPriceChange = -duration * initialPrice * dr;
            var currentPrice = GetBondPrice(rate + dr, t, f, c, 2);
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

        foreach (var r in rates.Select(r => r / 100.0))
        {
            var duration = GetBondDuration(r, t, f, c, k);
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

        foreach (var c in coupons.Select(c => c / 100.0))
        {
            var duration = GetBondDuration(r, t, f, c, k);
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

        foreach (var r in rates.Select(r => r / 100.0))
        {
            output.WriteLine($"interest rate = {r:P}");
            foreach (var t in maturities)
            {
                var duration = GetBondDuration(r, t, f, c, k);
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

        var portfolioValue = (double r) => portfolio.Sum(bond => GetBondPrice(r, bond.T, bond.F, bond.C));

        var initialPortfolioValue = portfolioValue(rate);
        var duration = portfolio.Sum(bond => GetBondDollarDuration(rate, bond.T, bond.F, bond.C, k)) / portfolioValue(rate);

        output.WriteLine($"initial portfolio value {initialPortfolioValue:N4}; duration {duration:N4}");

        foreach (var dr in drs.Select(dr => dr / 10_000.0))
        {
            var estimatedPortfolioValueChange = -duration * initialPortfolioValue * dr;
            var currentPortfolioValue = portfolioValue(rate + dr);
            var realPortfolioValueChange = currentPortfolioValue - initialPortfolioValue;

            output.WriteLine($"dr {dr:P4}; portfolio value {currentPortfolioValue:N4};" +
                             $" estimated change {estimatedPortfolioValueChange:N4};" +
                             $" real change {realPortfolioValueChange:N4};" +
                             $" estimation error {estimatedPortfolioValueChange / realPortfolioValueChange - 1:N4}");

            Math.Abs(estimatedPortfolioValueChange / realPortfolioValueChange - 1).Should().BeLessThan(.20);
        }
    }

    [Fact]
    public void HedgeWithDuration_Lecture69_Example1_CheckHowEffectiveHedgeIs()
    {
        var bond = (T: 10, F: 100_000, C: 4.0 / 100.0);
        //var hedge = (T: 10, F: ?, C: 0.0);
        var rate = 6.0 / 100.0;//6%
        var k = 1;
        var drs = new[] { 20, 100, 250, 400, 900 };//bps

        var priceBond = GetBondPrice(rate, bond.T, bond.F, bond.C);
        var durationBond = GetBondDuration(rate, bond.T, bond.F, bond.C, k);

        var priceHedge = GetBondPrice(rate, 10, 100, 0);//nominal value for face value
        var durationHedge = GetBondDuration(rate, 10, 1);//face value is cancelled out here, when C = 0

        var hedgeRatio = -priceBond * durationBond / priceHedge / durationHedge;

        var hedgeFaceValue = -100 * hedgeRatio;
        var realPriceHedge = GetBondPrice(rate, 10, hedgeFaceValue, 0);

        var initialPortfolioValue = priceBond - realPriceHedge;
        output.WriteLine($"bond price {priceBond:N4}; duration {durationBond:N4}");
        output.WriteLine($"hedge price {priceHedge:N4}; duration {durationHedge:N4}");
        output.WriteLine($"hedge ratio {hedgeRatio:N4}; real hedge price {realPriceHedge:N4}");
        output.WriteLine($"r {rate:P}; portfolio value {initialPortfolioValue:N4}; ");

        foreach (var dr in drs.Select(dr => dr / 10_000.0))
        {
            var currentPrice = GetBondPrice(rate + dr, bond.T, bond.F, bond.C);
            var currentHedge = GetBondPrice(rate + dr, 10, hedgeFaceValue, 0);
            var portfolioValue = currentPrice - currentHedge;

            output.WriteLine($"dr {dr:P4}; portfolio value {portfolioValue:N4}; " +
                             $" hedged loss {portfolioValue / initialPortfolioValue - 1:P4}; unhedged loss {currentPrice / priceBond - 1:P4}");
        }
    }

    [Fact]
    public void Immunization_HoldingReturn_Example01()
    {
        var faceValue = 100_000;
        var coupon = 11.5 / 100.0;//in %
        var maturity = 10;//in Years
        var rate = 6.0 / 100.0;//in %

        var initialPrice = GetBondPrice(rate, maturity, faceValue, coupon);
        output.WriteLine($"initial price {initialPrice}");

        foreach (var r in new[] { -200, 0, 200 }.Select(dr => rate + dr / 10_000.0))
        {
            output.WriteLine($"interest rate = {r:N4}");

            var holding5 = CalculateHoldingIncomeReturn(r, maturity, faceValue, coupon, initialPrice, 5);
            output.WriteLine($"holding 5 years: {holding5}");

            var holding7 = CalculateHoldingIncomeReturn(r, maturity, faceValue, coupon, initialPrice, 7);
            output.WriteLine($"holding 7 years: {holding7} - Macaulay duration is 7 years");

            var holding9 = CalculateHoldingIncomeReturn(r, maturity, faceValue, coupon, initialPrice, 9);
            output.WriteLine($"holding 9 years: {holding9}");
        }

        var duration = GetBondDuration(rate, maturity, faceValue, coupon);
        var macaulayDuration = GetBondMacaulayDuration(rate, maturity, faceValue, coupon);
        output.WriteLine($"duration {duration:N4}; Macaulay duration {macaulayDuration:N4}");
    }

    [Fact]
    public void Immunization_HoldingReturn_Example02()
    {
        var faceValue = 100_000;
        var maturity = 10;
        var coupon = 6 / 100.0;
        var yield = 4 / 100.0;

        var drs = new[] { 0, 50, 200 }
            .SelectMany(dr => new[] { -dr, dr })
            .Distinct()
            .OrderBy(dr => dr)
            .Select(dr => dr / 10_000.0)
            .ToArray();

        var macaulayDuration = (int)Math.Round(GetBondMacaulayDuration(yield, maturity, faceValue, coupon), 0);
        var initialPrice = GetBondPrice(yield, maturity, faceValue, coupon);

        output.WriteLine($"initial rate {yield:P}; price {initialPrice:N4}; Macaulay duration {macaulayDuration} (rounded up)");

        foreach (var dr in drs)
        {
            output.WriteLine($"dr {dr:P4}");

            var holding7 = CalculateHoldingIncomeReturn(yield + dr, maturity, faceValue, coupon, initialPrice, 7);
            var holding = CalculateHoldingIncomeReturn(yield + dr, maturity, faceValue, coupon, initialPrice, macaulayDuration);
            var holding9 = CalculateHoldingIncomeReturn(yield + dr, maturity, faceValue, coupon, initialPrice, 9);

            output.WriteLine($"holding period 7; {holding7}");
            output.WriteLine($"holding period {macaulayDuration}; {holding}");
            output.WriteLine($"holding period 9; {holding9}");
        }
    }

    private static dynamic CalculateHoldingIncomeReturn(
        double rate, int maturity, int faceValue, double coupon,
        double initialPrice,//one cannot calculate initial price here as interest rate is already different!
        int holdingYears)
    {
        var remainingPrice = GetBondPrice(rate, maturity - holdingYears, faceValue, coupon);

        var paidCoupon = GetCouponPayments(rate, holdingYears, faceValue, coupon);
        var holdingPosition = paidCoupon + remainingPrice;

        var holdingRate = Math.Pow(holdingPosition / initialPrice, 1.0 / holdingYears) - 1;

        return new
        {
            RemainingPrice = Math.Round(remainingPrice, 4),
            PaidCoupon = Math.Round(paidCoupon, 4),
            HoldingRate = Math.Round(holdingRate, 4)
        };
    }

    [Fact]
    public void GetHoldingPeriodReturn_SomeYears_Observe()
    {
        var F = 100_000;
        var c = 11.5 / 100.0 * F;
        var y = 6.0 / 100.0;
        var dy = 2.0 / 100.0;
        var k = 1;
        var T = 10;
        var t1 = 0;
        var t2 = 5;

        double N1 = k * (T - t1);
        double N2 = k * (T - t2);
        double Nh = k * (t2 - t1);

        // Price at purchase (t1)
        var P1 = c * (1 - Math.Pow(1 + y, -N1)) / y + F * Math.Pow(1 + y, -N1);

        // Price at sale (t2)
        var P2 = c * (1 - Math.Pow(1 + y + dy, -N2)) / (y + dy) + F * Math.Pow(1 + y + dy, -N2);

        // Future value of reinvested coupons at t2
        var FVc = c * (Math.Pow(1 + y + dy, Nh) - 1) / (y + dy);

        // Total Holding Period Return
        var hpr = Math.Pow((P2 + FVc) / P1, 1.0 / Nh) - 1;
        output.WriteLine($"holding period return {hpr:P}; p1 {P1:N4}; p2 {P2:N4}; coupons {FVc:N4}");
    }

    /// <summary>
    /// suppose you have 3_430_000 USD to invest for 6 years at 6.5% interest rate (flat term structure).
    /// which of the following bonds will you select as an investment instrument to achieve a goal of 5_000_000 in 6 years
    /// despite any (reasonable) parallel interest rate values shifts?
    ///
    /// so there are 2 assumptions: flat interest rate structure and reasonably small parallel shifts of it (+-3-5%)
    ///
    /// in the real world yield curve is not flat!
    /// and does not move as a whole!
    /// </summary>
    [Fact]
    public void SinglePaymentLiabilityExample_71_Observe()
    {
        var funds = 3_430_000;
        var rate = 6.5 / 100.0;
        var horizon = 6;
        var target = 5_000_000;

        var bonds = new[]
        {
            (T: 6, C: 6.5 / 100.0),
            (T: 12, C: 6.5 / 100.0),
            (T: 4, C: 6.5 / 100.0),
            (T: 7, C: 5.25 / 100.0)
        };

        var acceptedBonds = new List<(int maturity, double coupon, double value)>();

        foreach (var bond in bonds)
        {
            var faceValue = GetFaceValue(rate, bond.T, funds, bond.C);
            var macaulayDuration = (int)Math.Round(GetBondMacaulayDuration(rate, bond.T, faceValue, bond.C), 0);
            
            var coupons = GetCouponPayments(rate, macaulayDuration, faceValue, bond.C);
            var sellPrice = GetBondPrice(rate, bond.T - macaulayDuration, faceValue, bond.C);

            if (macaulayDuration <= horizon && coupons + sellPrice >= target) acceptedBonds.Add((bond.T, bond.C, coupons + sellPrice));

            output.WriteLine($"bond {bond.T} Y with {bond.C:P} coupon; face value {faceValue:N4};" +
                             $" Macaulay duration {macaulayDuration} Y;" +
                             $" value at MD: {coupons + sellPrice:N4}; to target {coupons + sellPrice - target:N4}");
        }

        output.WriteLine("\n\nbonds which meets the time and funds target and is immune to interest rate jumps (at least linear exposure)");
        foreach (var (t,c,v) in acceptedBonds)
        {
            output.WriteLine($"bond with maturity {t} Y and coupon {c:P} gives value {v:N4} ");
        }
        output.WriteLine("\n\nmajor simplification here - parallel shifts in rates shocks - in the real world yield curve is not flat!!!!!");
    }

    [Fact]
    public void RateLevelTrading_2Bonds_MaxProfit()
    {
        var f1 = 20_000;
        var t1 = 3;
        var c1 = 3 / 100.0;
        
        var rate = 5 / 100.0;
        var h = 3;

        var t2 = 20;
        var c2 = 2 / 100.0;

        var currentPrice1 = GetBondPrice(rate, t1, f1, c1);
        var duration1 = GetBondDuration(rate, t1, f1, c1);
        var macaulayDuration1 = GetBondMacaulayDuration(rate, t1, f1, c1);

        var currentNominalPrice2 = GetBondPrice(rate, t2, 100, c2);
        var scale = currentPrice1 / currentNominalPrice2;
        var f2 = 100 * scale;
        var duration2 = GetBondDuration(rate, t2, f2, c2);
        var macaulayDuration2 = GetBondMacaulayDuration(rate, t2, f2, c2);

        var holdedPrice2 = GetBondPrice(rate - 2/100.0, t2-h, f2, c2);
        var cumulativeCouponCompounding = (1.03 * 1.03 + 1.03 + 1);

        var holdCurrent = f1 + c1 * f1 * cumulativeCouponCompounding;//as rate drops by 2% from 5 to 3, and there are 3 coupons
        var switchToLong = holdedPrice2 + c2 * f2 * cumulativeCouponCompounding;

        output.WriteLine(
            $"""
             strategy 1 - hold current bond
                capital in {h} years = {holdCurrent:N4} = face value + coupons reinvested = {f1} + {holdCurrent - f1:N4}
                
             strategy 2 - sell current bond and buy the alternative 
                first one today has price {currentPrice1:N4} and the alternative's nominal price is {currentNominalPrice2:N4} for 100 face value
                so we can buy {scale:N0} items of the alternative bond today
                
                after {h} years we can sell the alternative bond for {holdedPrice2:N4} and accumulate coupons for {switchToLong - holdedPrice2:N4}
                thus the capital is {switchToLong:N4}
                
             current bond's duration {duration1:N4} and Macaulay duration {macaulayDuration1:N4}
             alternative's  duration {duration2:N4} and Macaulay duration {macaulayDuration2:N4}
             
             as we expect the interest rate to fall (from 5% to 3%) 
             the profitable strategy is to switch from short duration (initial bond) to long duration (the alternative bond),
             which we proved by the actual calculations.
             """);
    }

    [Fact]
    public void GetConvexity_RepricingVsLinearVsSecondOrderApproximation_Observe()
    {
        var maturity = 5;
        var coupon = 4 / 100.0;
        var faceValue = 25_000;
        var rate = 5 / 100.0;
        var drs = new[] { -100, +100 }.Select(dr => dr / 10_000.0).ToArray();

        var price = GetBondPrice(rate, maturity, faceValue, coupon);
        var dollarDuration = GetBondDollarDuration(rate, maturity, faceValue, coupon);
        var convexity = GetBondDollarConvexity(rate, maturity, faceValue, coupon);

        foreach (var dr in drs)
        {
            var currentPrice = GetBondPrice(rate + dr, maturity, faceValue, coupon);
     
            var actual = currentPrice - price;
            var linear = -dr * dollarDuration;
            var secondOrder = -dr * dollarDuration + dr * dr / 2 * convexity;

            output.WriteLine($"dr {dr:P4}, actual {actual:N4}; linear {linear:N4}; second order {secondOrder:N4}");
        }
    }
}