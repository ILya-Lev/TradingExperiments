using Xunit.Abstractions;

namespace BlackScholesMerton.Tests;

public class EuropeanOptionOnFuturesCalculatorTests(ITestOutputHelper output)
{
    /// <summary>
    /// scenario:
    /// today future price is 100; you sell European call option on the future for 10% over the fair price with maturity 6m
    /// then you hedge it with delta hedging
    ///
    /// in 1 month you re-balance your portfolio according to the new delta value; at the moment future price is 98
    ///
    /// in 2 month you liquidate the portfolio: sell the future position, buy back the option position and close bank position
    ///
    /// target question - what is your PNL (fees are ignored)
    /// </summary>
    [Fact]
    public void EuropeanOption_1mReplicate_2mLiquidate_TotalPnl()
    {
        var calculators = new[]
        {
            new EuropeanOptionOnFuturesCalculator(100, 100, 0.05, 0.25, 1 / 2.0),
            new EuropeanOptionOnFuturesCalculator(98, 100, 0.05, 0.25, 5 / 12.0),
            new EuropeanOptionOnFuturesCalculator(101, 100, 0.05, 0.25, 1 / 3.0),
        };
        var riskFreeGrowthPerMonth = Math.Exp(0.05 / 12);

        var optionInitial = 1.10 * calculators[0].CallPrice;
        var buyFutureInitial = calculators[0].CallDelta * calculators[0].F;
        var bankInitial = optionInitial - buyFutureInitial;

        var reBalanceCost = (calculators[1].CallDelta - calculators[0].CallDelta) * calculators[1].F;
        var bankReBalance = bankInitial * riskFreeGrowthPerMonth - reBalanceCost;

        var liquidateOption = calculators[2].CallPrice;
        var liquidateFutures = calculators[1].CallDelta * calculators[2].F;
        var liquidateBank = bankReBalance * riskFreeGrowthPerMonth;

        var pnl = -liquidateOption + liquidateFutures + liquidateBank;

        output.WriteLine(
            $"""
             initial:
                     option cost {optionInitial:N4}
                     futures # {calculators[0].CallDelta:N4}
                     buy futures {buyFutureInitial:N4}
                     bank {bankInitial:N4}
             re balance:
                     futures # {calculators[1].CallDelta:N4}
                     buy futures {reBalanceCost:N4}
                     bank {bankReBalance:N4}
             liquidate:
                     option {liquidateOption:N4}
                     sell futures {liquidateFutures:N4}
                     bank {liquidateBank:N4}
             pnl: {pnl:N4}
             """
        );
    }
}