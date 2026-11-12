using FluentAssertions;

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
}