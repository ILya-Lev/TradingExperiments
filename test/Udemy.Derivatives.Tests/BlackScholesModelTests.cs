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

        var oneStepPnL = scale*(callAfter - callBefore) - sharesPosition*(spotAfter - spotBefore);

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

        var oneStepPnL = scale*(callAfter - callBefore) - sharesPosition*(spotAfter - spotBefore);

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
        var gammaHedgePosition = (int)Math.Ceiling(callPosition * (delta - gamma * initialSpot/2));

        var finalBsm = new BlackScholesModel(finalSpot, 0.04, 0.15);
        var finalCallValue = finalBsm.GetPrice(call, 9.0 / 12);

        var pnl = callPosition * (finalCallValue - initialCallValue) - hedgePosition * (finalSpot - initialSpot);
        var pnlGamma = callPosition * (finalCallValue - initialCallValue) - gammaHedgePosition * (finalSpot - initialSpot);

        output.WriteLine(
            $"""
            initial state: 
                call position {callPosition}
                call price {initialCallValue:N4}
                delta {delta:N4}
                hedge position {hedgePosition}
                spot price {initialSpot}
                
            3 months later state:
                call price {finalCallValue:N4}
                spot price {finalSpot}
            
            PnL {pnl:N4}
            
            with gamma in mind
                gamma {gamma:N4}
                gamma hedge position {gammaHedgePosition}
                PnL with gamma {pnlGamma:N4}
            """);
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
        var oneStepPnL = - scale * (callAfter - callBefore) + sharesPosition * (spotAfter - spotBefore);

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
    [InlineData(100, 5, 100, 4.1923, 0.5,10)]
    [InlineData(110, 5, 100, 12.6024, 0.5, 10)]
    [InlineData(90, 5, 100, 0.4226, 0.5, 10)]
    [InlineData(110, 4, 105, 11.8802, 1, 15)]
    [InlineData(100, 4, 105, 4.3340, .75, 15)]
    [InlineData(120, 4, 105, 18.8155, .75, 15)]
    [InlineData(72, 4, 60, 13.5028, .5, 20)]
    [InlineData(60, 5, 50, 11.4762, .5, 20)]
    [InlineData(70, 5, 50, 21.0373, 5.0/12, 20)]
    [InlineData(50, 5, 50, 3.0953, 5.0/12, 20)]
    public void GetImpliedVolatility_FromExamplesAbove_MatchExpected(
        double spot,
        double interestRate,
        double strike, 
        double callPrice, 
        double timeToMaturity,
        double expectedVolatility)
    {
        var calculator = new ImpliedVolatilityCalculator(spot, interestRate/100.0);

        var impliedVolatility = calculator.GetImpliedVolatility(callPrice, strike, timeToMaturity);

        impliedVolatility.Should().BeApproximately(expectedVolatility/100.0, 1e-3);
        output.WriteLine
        (
            $"S {spot}, r {interestRate / 100.0:N2}, K {strike}, T-t {timeToMaturity}, C {callPrice:N4}" +
            $", ev {expectedVolatility / 100.0:N2} vs iv {impliedVolatility:N6}"
        );
    }

}