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
}