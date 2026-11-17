using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class BlackScholesModelTests(ITestOutputHelper output)
{
    [Fact]
    public void DemonstrateCallTimeDecay_ViaTheta_MatchRepricing()
    {
        var call = new CallOption(25, 2.0 / 12);
        var bsm = new BlackScholesModel(30, 0.04, 0.35);

        var theta = bsm.GetTheta(call, 2.0 / 12);
        var callBefore = bsm.GetPrice(call, 2.0 / 12);
        var callAfter = bsm.GetPrice(call, 1.0 / 12);
        var callPriceChange = callAfter - callBefore;
        var predictedPriceChange = theta * 1.0 / 12;

        callPriceChange.Should().BeApproximately(predictedPriceChange, 2e-2);

        output.WriteLine(
            $"""
                call price 2 months before maturity {callBefore:N4}
                call price 1 months before maturity {callAfter:N4}
                
                actual price change {callPriceChange:N4}
                
                theta {theta:N4}
                predicted price change {predictedPriceChange:N4}
                
                error {Math.Abs(predictedPriceChange - callPriceChange):N4}
            """);
    }

    [Theory]
    [InlineData(60, 70)]
    [InlineData(60, 50)]
    public void DeltaHedgingDemo_Delta_Pnl(double spotBefore, double spotAfter)
    {
        var scale = 200;
        var call = new CallOption(50, 6.0 / 12);

        var bsmBefore = new BlackScholesModel(spotBefore, 0.05, 0.20);
        var callBefore = bsmBefore.GetPrice(call, 6.0 / 12);
        var deltaBefore = bsmBefore.GetDelta(call, 6.0 / 12);
        var sharesPosition = Math.Ceiling(scale * deltaBefore);

        var bsmAfter = new BlackScholesModel(spotAfter, 0.05, 0.20);
        var callAfter = bsmAfter.GetPrice(call, 5.0 / 12);
        var deltaAfter = bsmAfter.GetDelta(call, 5.0 / 12);

        var oneStepPnL = scale * (callAfter - callBefore) - sharesPosition * (spotAfter - spotBefore);

        output.WriteLine(
            $"""
                6 months before maturity spot {spotBefore}
                    call price {callBefore:N4}
                    delta {deltaBefore:N4}
                    hedging position {sharesPosition}
                
                5 months before maturity spot {spotAfter}
                    call price {callAfter:N4}
                    delta {deltaAfter:N4}
                    
                PnL {oneStepPnL:N4}
            """);
    }

    [Theory]
    [InlineData(100, 90)]
    [InlineData(100, 100)]//PnL is similar in all cases as mostly comes from volatility increase
    [InlineData(100, 110)]
    public void LongVolatilityTrading_StaticDeltaHedging_Pnl(double spotBefore, double spotAfter)
    {
        var scale = 1000;
        var call = new CallOption(100, 1);

        var bsmBefore = new BlackScholesModel(spotBefore, 0.05, 0.10);
        var callBefore = bsmBefore.GetPrice(call, 1);
        var deltaBefore = bsmBefore.GetDelta(call, 1);
        var sharesPosition = Math.Ceiling(scale * deltaBefore);

        var bsmAfter = new BlackScholesModel(spotAfter, 0.05, 0.40);
        var callAfter = bsmAfter.GetPrice(call, 0.5);
        var deltaAfter = bsmAfter.GetDelta(call, 0.5);

        var oneStepPnL = scale * (callAfter - callBefore) - sharesPosition * (spotAfter - spotBefore);

        output.WriteLine(
            $"""
                12 months before maturity spot {spotBefore}
                    call price {callBefore:N4}
                    delta {deltaBefore:N4}
                    hedging position {sharesPosition}
                
                6 months before maturity spot {spotAfter}
                    call price {callAfter:N4}
                    delta {deltaAfter:N4}
                    
                PnL {oneStepPnL:N4}
            """);
    }

    [Fact]
    public void ComputeGreeks_Problem10_4_Observe()
    {
        var call = new CallOption(60, 0.5);
        var bsm = new BlackScholesModel(72, 0.04, 0.20);

        var delta = bsm.GetDelta(call, 0.5);
        var gamma = bsm.GetGamma(call, 0.5);
        var vega = bsm.GetVega(call, 0.5);
        var theta = bsm.GetTheta(call, 0.5);

        using var _ = new AssertionScope();
        delta.Should().BeApproximately(.9334, 1e-4);
        gamma.Should().BeApproximately(.0127, 1e-4);
        vega.Should().BeApproximately(6.5807, 1e-4);
        theta.Should().BeApproximately(-3.4641, 1e-4);

        output.WriteLine(
            $"""
            call price {bsm.GetPrice(call, 0.5):N4}
            delta {delta:N4}
            gamma {gamma:N4}
            vega {vega:N4}
            theta {theta:N4}
            """);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(120)]
    public void StaticDeltaHedging_Problem10_5_Observe(double finalSpot)
    {
        var callPosition = 50;
        var initialSpot = 110;
        var call = new CallOption(105, 1);

        var initialBsm = new BlackScholesModel(initialSpot, 0.04, 0.15);
        var initialCallValue = initialBsm.GetPrice(call, 1);
        var delta = initialBsm.GetDelta(call, 1);
        var gamma = initialBsm.GetGamma(call, 1);
        var hedgePosition = (int)Math.Ceiling(callPosition * delta);
        //to include gamma into the account either use 2 options (like in convexity hedging of bonds)
        //or do dynamic delta hedging for short time intervals (i.e., daily, although transaction cost might make it useless)
        //var gammaHedgePosition = callPosition * (delta + gamma * initialSpot/2);

        var finalBsm = new BlackScholesModel(finalSpot, 0.04, 0.15);
        var finalCallValue = finalBsm.GetPrice(call, 9.0 / 12);

        var pnl = callPosition * (finalCallValue - initialCallValue) - hedgePosition * (finalSpot - initialSpot);
        //var pnlGamma = callPosition * (finalCallValue - initialCallValue) - gammaHedgePosition * (finalSpot - initialSpot);

        output.WriteLine(
            $$"""
            initial state: 
                call position {{callPosition}}
                call price {{initialCallValue:N4}}
                delta {{delta:N4}}
                gamma {gamma:N4}
                hedge position {{hedgePosition}}
                spot price {{initialSpot}}
                
            3 months later state:
                call price {{finalCallValue:N4}}
                spot price {{finalSpot}}
            
            PnL {{pnl:N4}}
            """);
        /*with gamma in mind
             gamma hedge position {gammaHedgePosition:N4}
             PnL with gamma {pnlGamma:N4}*/
    }

    [Theory]
    [InlineData(100, 90)]
    [InlineData(100, 100)]//PnL is similar in all cases as mostly comes from volatility increase
    [InlineData(100, 110)]
    public void ShortVolatilityTrading_StaticDeltaHedging_Pnl(double spotBefore, double spotAfter)
    {
        var scale = 1000;
        var call = new CallOption(100, 1);

        var bsmBefore = new BlackScholesModel(spotBefore, 0.05, 0.40);
        var callBefore = bsmBefore.GetPrice(call, 1);
        var deltaBefore = bsmBefore.GetDelta(call, 1);
        var sharesPosition = Math.Ceiling(scale * deltaBefore);

        var bsmAfter = new BlackScholesModel(spotAfter, 0.05, 0.10);
        var callAfter = bsmAfter.GetPrice(call, 0.5);
        var deltaAfter = bsmAfter.GetDelta(call, 0.5);

        //as we expect volatility to drop, we go short on call option
        //but as we want to reduce underlying asset price exposure, we take the opposite position in delta hedging
        var oneStepPnL = -scale * (callAfter - callBefore) + sharesPosition * (spotAfter - spotBefore);

        output.WriteLine(
            $"""
                 12 months before maturity spot {spotBefore}
                     call price {callBefore:N4}
                     delta {deltaBefore:N4}
                     hedging position {sharesPosition}
                 
                 6 months before maturity spot {spotAfter}
                     call price {callAfter:N4}
                     delta {deltaAfter:N4}
                     
                 PnL {oneStepPnL:N4}
             """);
    }

    [Theory]
    [InlineData(100, 5, 100, 18.0230, 1, 40)]
    [InlineData(100, 5, 100, 4.1923, 0.5, 10)]
    [InlineData(110, 5, 100, 12.6024, 0.5, 10)]
    [InlineData(90, 5, 100, 0.4226, 0.5, 10)]
    [InlineData(110, 4, 105, 11.8802, 1, 15)]
    [InlineData(100, 4, 105, 4.3340, .75, 15)]
    [InlineData(120, 4, 105, 18.8155, .75, 15)]
    [InlineData(72, 4, 60, 13.5028, .5, 20)]
    [InlineData(60, 5, 50, 11.4762, .5, 20)]
    [InlineData(70, 5, 50, 21.0373, 5.0 / 12, 20)]
    [InlineData(50, 5, 50, 3.0953, 5.0 / 12, 20)]
    public void GetImpliedVolatility_FromExamplesAbove_MatchExpected(
        double spot,
        double interestRate,
        double strike,
        double callPrice,
        double timeToMaturity,
        double expectedVolatility)
    {
        var calculator = new ImpliedVolatilityCalculator(spot, interestRate / 100.0);

        var impliedVolatility = calculator.GetImpliedVolatility(callPrice, strike, timeToMaturity);

        impliedVolatility.Should().BeApproximately(expectedVolatility / 100.0, 1e-3);
        output.WriteLine
        (
            $"S {spot}, r {interestRate / 100.0:N2}, K {strike}, T-t {timeToMaturity}, C {callPrice:N4}" +
            $", ev {expectedVolatility / 100.0:N2} vs iv {impliedVolatility:N6}"
        );
    }


    [Theory]
    [InlineData(160)]//call at the money
    [InlineData(170)]//call out of the money
    [InlineData(150)]//call in the money
    public void DeltaExposure_ComputationalProblemSet3_2_CallDifferentMoneyness(double strike)
    {
        var scale = 100;
        var spot = 160;
        var call = new CallOption(strike, 2 / 12.0);
        var bsm = new BlackScholesModel(spot, 0.06, 0.15);

        var callPrice = bsm.GetPrice(call, 2.0 / 12);
        var enterPosition = scale * callPrice;
        var underlyingCost = spot * scale;

        output.WriteLine(strike < spot ? "in the money" : strike > spot ? "out of the money" : "at the money");
        output.WriteLine($"enter CALL options position {enterPosition:N4} vs enter underlying position {underlyingCost:N4}");

        var delta = bsm.GetDelta(call, 2.0 / 12);
        var ds1 = 10;
        var estimatedCallChangeUp = ds1 * delta;
        var actualCallChangeUp = bsm.GetPrice(call, 1.0 / 12, spot + ds1) - callPrice;
        var delta1 = bsm.GetDelta(call, 1.0 / 12, spot + ds1);
        var ds2 = -10;
        var estimatedCallChangeDown = ds2 * delta;
        var actualCallChangeDown = bsm.GetPrice(call, 1.0 / 12, spot + ds2) - callPrice;
        var delta2 = bsm.GetDelta(call, 1.0 / 12, spot + ds2);

        output.WriteLine(
            $"""
            delta {delta:N4}
            up {ds1} => estimated {estimatedCallChangeUp:N4} vs real {actualCallChangeUp:N4} jump; new delta {delta1:N4}
            down {ds2} => estimated {estimatedCallChangeDown:N4} vs real {actualCallChangeDown:N4} jump; new delta {delta2:N4}
            """);

        var theta = bsm.GetTheta(call, 2.0 / 12);
        var callInOneMonth = bsm.GetPrice(call, 1.0 / 12);
        var timeDecayExpected = callPrice + theta * 1.0 / 12;
        output.WriteLine($"theta {theta:N4} per Y or {theta / 365:N4} per D;" +
                         $" expected time decay {timeDecayExpected:N4} vs recalculated {callInOneMonth:N4}");

        var vega = bsm.GetVega(call, 2.0 / 12);
        output.WriteLine($"vega {vega:N4}");

        var spotRange = Enumerable.Range(spot - 30, 61).Select(s => (double)s).ToArray();
        var callRange = spotRange.Select(s => bsm.GetPrice(call, 2.0 / 12, s)).ToArray();
        var points = spotRange.Zip(callRange, (s, c) => ((decimal)s, (decimal)c)).ToArray();
        var plot = PlotHelper.PlotSeries($"call (K={strike}) vs spot price", points, false);
        output.WriteLine($"call (K={strike}) vs spot price {plot.Path}");
    }

    [Theory]
    [InlineData(160)]//call at the money
    [InlineData(170)]//call in the money
    [InlineData(150)]//call out of the money
    public void DeltaExposure_ComputationalProblemSet3_2_PutDifferentMoneyness(double strike)
    {
        var scale = 100;
        var spot = 160;
        var call = new PutOption(strike, 2 / 12.0);
        var bsm = new BlackScholesModel(spot, 0.06, 0.15);

        var callPrice = bsm.GetPrice(call, 2.0 / 12);
        var enterPosition = scale * callPrice;
        var underlyingCost = spot * scale;

        output.WriteLine(strike > spot ? "in the money" : strike < spot ? "out of the money" : "at the money");
        output.WriteLine($"enter PUT options position {enterPosition:N4} vs enter underlying position {underlyingCost:N4}");

        var delta = bsm.GetDelta(call, 2.0 / 12);
        var ds1 = 10;
        var estimatedCallChangeUp = ds1 * delta;
        var actualCallChangeUp = bsm.GetPrice(call, 1.0 / 12, spot + ds1) - callPrice;
        var delta1 = bsm.GetDelta(call, 1.0 / 12, spot + ds1);
        var ds2 = -10;
        var estimatedCallChangeDown = ds2 * delta;
        var actualCallChangeDown = bsm.GetPrice(call, 1.0 / 12, spot + ds2) - callPrice;
        var delta2 = bsm.GetDelta(call, 1.0 / 12, spot + ds2);

        output.WriteLine(
            $"""
            delta {delta:N4}
            up {ds1} => estimated {estimatedCallChangeUp:N4} vs real {actualCallChangeUp:N4} jump; new delta {delta1:N4}
            down {ds2} => estimated {estimatedCallChangeDown:N4} vs real {actualCallChangeDown:N4} jump; new delta {delta2:N4}
            """);

        var theta = bsm.GetTheta(call, 2.0 / 12);
        var callInOneMonth = bsm.GetPrice(call, 1.0 / 12);
        var timeDecayExpected = callPrice + theta * 1.0 / 12;
        output.WriteLine($"theta {theta:N4} per Y or {theta / 365:N4} per D;" +
                         $" expected time decay {timeDecayExpected:N4} vs recalculated {callInOneMonth:N4}");

        var vega = bsm.GetVega(call, 2.0 / 12);
        output.WriteLine($"vega {vega:N4}");

        var spotRange = Enumerable.Range(spot - 30, 61).Select(s => (double)s).ToArray();
        var callRange = spotRange.Select(s => bsm.GetPrice(call, 2.0 / 12, s)).ToArray();
        var points = spotRange.Zip(callRange, (s, c) => ((decimal)s, (decimal)c)).ToArray();
        var plot = PlotHelper.PlotSeries($"put (K={strike}) vs spot price", points, false);
        output.WriteLine($"put vs spot price {plot.Path}");
    }

    [Theory]
    [InlineData(90)]
    [InlineData(100)]
    [InlineData(110)]
    public void VolatilityTrading_ComputationalProblemSet3_LongVolatility(double spot)
    {
        var scale = 1_000;
        var initialSpot = 100;
        var call = new CallOption(initialSpot, 1);//initially the option is at the money => K = S

        var initialBsm = new BlackScholesModel(initialSpot, 0.05, 0.10);
        var initialCallPrice = initialBsm.GetPrice(call, 1);
        var delta = initialBsm.GetDelta(call, 1);
        var vega = initialBsm.GetVega(call, 1);
        var theta = initialBsm.GetTheta(call, 1);
        var hedgePosition = (int)Math.Ceiling(delta * scale);

        var finalBsm = new BlackScholesModel(spot, 0.05, 0.40);
        var finalCallPrice = finalBsm.GetPrice(call, 0.5);
        var threeDaysCallPrice = finalBsm.GetPrice(call, 362 / 365.0);

        var pnl = scale * (finalCallPrice - initialCallPrice) - hedgePosition * (spot - initialSpot);
        var pnlOptionsOnly = scale * (finalCallPrice - initialCallPrice);
        var maxPnlOptionsOnly = scale * (finalBsm.GetPrice(call with { Strike = 106 }, 0.5)
                                         - initialBsm.GetPrice(call with { Strike = 106 }, 1));
        var timeDecay = theta * 6.0 / 12 * scale;
        var threeDaysPnl = scale * (threeDaysCallPrice - initialCallPrice) - hedgePosition * (spot - initialSpot);

        var strikes = Enumerable.Range(70, 61).Select(k => (double)k).ToArray();
        var vegas = strikes.Select(k => initialBsm.GetVega(call with { Strike = k }, 1)).ToArray();
        var points = strikes.Zip(vegas, (k, v) => ((decimal)k, (decimal)v)).ToArray();
        var plot = PlotHelper.PlotSeries("vegas", points, false);
        output.WriteLine($"vegas plot: {plot.Path}\n\n");

        output.WriteLine(
            $"""
            1 year to maturity
                call price {initialCallPrice:N4}              
                delta {delta:N4}
                hedge position {hedgePosition}
                vega {vega:N4} (we expect the volatility to grow)
                
            6 months to maturity
                call price {finalCallPrice:N4}
            
            PnL {pnl:N4}
            PnL options only {pnlOptionsOnly:N4}
            Max PnL options only {maxPnlOptionsOnly:N4}
            Three days PnL with hedging {threeDaysPnl:N4}
            
            time decay {timeDecay:N4}
            """);
    }

    [Fact]
    public void VolatilityTrading_ComputationalProblemSet3_ShortVolatility()
    {
        var scale = 500;
        var spot = 80;

        var initialBsm = new BlackScholesModel(spot, 0.06, 0.35);
        var call = new CallOption(0, 1.0 / 12);

        var sampleStrikes = new double[] { 70, 80, 90 };
        foreach (var strike in sampleStrikes)
        {
            var currentCall = call with { Strike = strike };

            var initialCallPrice = initialBsm.GetPrice(currentCall, 1.0 / 12);
            var delta = initialBsm.GetDelta(currentCall, 1.0 / 12);
            var gamma = initialBsm.GetGamma(currentCall, 1.0 / 12);
            var vega = initialBsm.GetVega(currentCall, 1.0 / 12);
            var hedgePosition = (int)Math.Ceiling(delta * scale);

            output.WriteLine(
                $"""
                 1 month to maturity with strike {strike} and spot {spot}
                     call price {initialCallPrice:N4}              
                     delta {delta:N4}
                     hedge position {hedgePosition}
                     vega {vega:N4} (we expect the volatility to fall)
                     gamma x 500 {scale*gamma:N4}
                 """);

            var spots = new double[] { 81, 83, 85, 90, 79, 77, 75, 70 };
            foreach (var s in spots)
            {
                var finalBsm = new BlackScholesModel(s, 0.06, 0.20);
                var finalCallPrice = finalBsm.GetPrice(currentCall, 29 / 365.0);
                var pnl = -scale * (finalCallPrice - initialCallPrice) + hedgePosition * (s - spot);
                output.WriteLine($"spot {s} pnl {pnl:N4}");
            }
        }

        var strikes = Enumerable.Range(50, 61).Select(k => (double)k).ToArray();
        var vegas = strikes.Select(k => initialBsm.GetVega(call with { Strike = k }, 1)).ToArray();
        var points = strikes.Zip(vegas, (k, v) => ((decimal)k, (decimal)v)).ToArray();
        var plot = PlotHelper.PlotSeries("vegas short", points, false);
        output.WriteLine($"vegas short plot: {plot.Path}\n\n");

    }
}