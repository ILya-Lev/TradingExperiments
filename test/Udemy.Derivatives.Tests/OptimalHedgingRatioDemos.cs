using FluentAssertions;
using FluentAssertions.Execution;
using MathNet.Numerics.Statistics;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class OptimalHedgingRatioDemos(ITestOutputHelper output)
{
    [Fact]
    public void OptimalHedgeRatio_Example33_Observe()
    {
        double[] assetChanges = [.029, .02, -.044, .008, .026, -.019, -.01, -.007, .043, .011, -.036, -.018, .009, -.032, .023];
        double[] futuresChanges = [.021, .035, -.046, .001, .044, -.029, -.026, -.029, .048, -.006, -.036, -.011, .019, -.027, .029];
        double targetAssetQuantity = 2_000_000;
        double contractSize = 42_000;

        var assetStat = new DescriptiveStatistics(assetChanges, true);
        var sigmaS = assetStat.StandardDeviation;

        var futuresStat = new DescriptiveStatistics(futuresChanges, true);
        var sigmaF = Math.Abs(futuresStat.StandardDeviation) < 1e-6 ? 1 : futuresStat.StandardDeviation;

        var correlation = Correlation.Pearson(assetChanges, futuresChanges);

        var optimalHedgeRatio = correlation * sigmaS / sigmaF;
        var optimalContractNumber = optimalHedgeRatio * targetAssetQuantity / contractSize;

        output.WriteLine($"sigma s = {sigmaS:N4} sigma f = {sigmaF:N4} rho {correlation:N4}");
        output.WriteLine($"optimal hedge ratio = {optimalHedgeRatio:N4} optimal contract number = {optimalContractNumber:N4}");

        using var _ = new AssertionScope();
        assetChanges.Length.Should().Be(futuresChanges.Length);
        optimalHedgeRatio.Should().BeApproximately(.78, 1e-2);
        optimalContractNumber.Should().BeApproximately(37, 5e-1);
    }

}